namespace Orleankka.Http

open Orleankka
open Orleankka.Typed
open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection

module MediaType = 

 type MediaType =
   | Actor
   | TypedActor
 
 let contentTypeActor = "orleankka/vnd.actor+json"
 let contentTypeTypedActor = "orleankka/vnd.typed.actor+json"

 let parseMediaType (contentType:string) =
   if contentType.Equals(contentTypeActor, StringComparison.InvariantCultureIgnoreCase) 
      then Actor |> Some
   else if contentType.Equals(contentTypeTypedActor, StringComparison.InvariantCultureIgnoreCase)
      then TypedActor |> Some
   else None
   

module MessageType =

 type MessageType =   
   | Class of msgType:Type
   | DU of msgType:Type
   | Typed

 let mapToDU(caseName, msgBody) = sprintf "Case: %s %s" caseName msgBody
 
 let mapToTyped(memberName, msgBody) = sprintf "Member: %s %s" memberName msgBody
 

module HttpRoute =
 
 open MessageType

 type Route = {
   HttpPath : string
   MsgType : MessageType
   ActorRef : ActorRef
 }

 let createHttpPath(actorName, id, msgName) = (sprintf "%s/%s/%s" actorName id msgName).ToLowerInvariant()

 let create (msgType:MessageType, actorPath:ActorPath) = 
   let actorRef = ActorRef.Deserialize(actorPath)
   match msgType with   
   
   | Class t -> [| { HttpPath = createHttpPath(actorPath.Type.Name, actorPath.Id, t.Name)
                     MsgType = msgType
                     ActorRef = actorRef } |]
                       
   | DU t -> FSharpType.GetUnionCases(t) 
             |> Array.map(fun case ->                
                { HttpPath = createHttpPath(actorPath.Type.Name, actorPath.Id, case.Name)
                  MsgType = msgType
                  ActorRef = actorRef })

   | Typed -> actorPath.Type.GetMembers()
              |> Array.map(fun membeR ->               
                 { HttpPath = createHttpPath(actorPath.Type.Name, actorPath.Id, membeR.Name)
                   MsgType = msgType
                   ActorRef = actorRef })   


module ActorRouter =

 open MessageType
 open HttpRoute 

 type Router(deserialize:string*Type -> obj, routes:IDictionary<string,Route>) =   
   
   let getCaseName (path:string) = path
   let getMemberName (path:string) = path

   member this.Dispatch(httpPath, msg) =            
      match routes.TryGetValue(httpPath) with      
      | (true, route) ->
         let message = match route.MsgType with
                       | Class t -> deserialize(msg, t)
                       
                       | DU t -> let caseName = msg |> getCaseName
                                 let duMsg = MessageType.mapToDU(caseName, msg)
                                 deserialize(duMsg, t)

                       | Typed -> let memberName = msg |> getMemberName
                                  let typedMsg = MessageType.mapToTyped(memberName, msg)
                                  deserialize(typedMsg, typeof<Invocation>)

         route.ActorRef.Ask(message) |> Some
      | _  -> None

 let create deserialize (paths:Route seq) =
   let dic = paths |> Seq.map(fun p -> (p.HttpPath,p)) |> dict
   Router(deserialize, dic)