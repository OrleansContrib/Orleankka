namespace Orleankka.Http

open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open Orleankka

module ContentType = 

 let Orleankka = "orleankka/vnd.actor+json"    

module MessageType =

 type MessageType =   
   | Class of msgType:Type
   | DU of msgType:Type

 let mapToDU(caseName, msgBody) = sprintf "{ 'Case': '%s', 'Fields': [%s] }" caseName msgBody
 
 let mapToTyped(memberName, msgBody) = sprintf "{ Member: %s %s }" memberName msgBody
 

module HttpRoute =
 
 open MessageType

 type Route = {
   HttpPath : string
   MsgType : MessageType
   ActorPath : ActorPath
 }

 let createHttpPath(actorName, id, msgName) = (sprintf "%s/%s/%s" actorName id msgName)

 let create (msgType, actorPath:ActorPath) = 
   match msgType with   
   
   | Class t -> [| { HttpPath = createHttpPath(actorPath.Type, actorPath.Id, t.Name)
                     MsgType = msgType
                     ActorPath = actorPath } |]
                       
   | DU t -> FSharpType.GetUnionCases(t) 
             |> Array.map(fun case ->                
                { HttpPath = createHttpPath(actorPath.Type, actorPath.Id, case.Name)
                  MsgType = msgType
                  ActorPath = actorPath })

module ActorRouter =

 open MessageType
 open HttpRoute 
 open System.Linq

 type Router(deserialize:string*Type -> obj, routes:IDictionary<string,Route>) =   
      
   let getMessageName (path:string) = path.Split('/').Last()

   member this.Dispatch(system:IActorSystem, httpPath, msg) =            
      match routes.TryGetValue(httpPath) with      
      | (true, route) ->
         let message = match route.MsgType with
                       | Class t -> deserialize(msg, t)
                       
                       | DU t -> let caseName = httpPath |> getMessageName
                                 let duMsg = MessageType.mapToDU(caseName, msg)
                                 deserialize(duMsg, t)

         system.ActorOf(route.ActorPath).Ask<obj>(message) |> Some
      | _  -> None

 let create deserialize (paths:Route seq) =
   let dic = paths |> Seq.map(fun p -> (p.HttpPath,p)) |> dict
   Router(deserialize, dic)