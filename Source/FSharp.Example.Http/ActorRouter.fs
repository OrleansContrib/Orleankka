module ActorRouter

open Orleankka
open Orleankka.Typed

open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection

type Result<'TSuccess,'TFailure> = 
   | Success of 'TSuccess
   | Failure of 'TFailure


type MediaType =
   | VndActor
   | VndTypedActor
   with 
   static member VndActorJson = "orleankka/vnd.actor+json"
   static member VndTypedActorJson = "orleankka/vnd.typed.actor+json"


type MessageType =
   | General of Type
   | DU of Type
   | Typed of Type
   with 
   static member CreateDU(caseName, msg) = sprintf "Case: %s %s" caseName msg
   static member CreateTyped(memberName, msg) = sprintf "Member: %s %s" memberName msg


type HttpRoute = {
   Path : string
   MsgType : MessageType
   Actor : ActorRef
} with
  static member CreatePath(actor, id, msgType) = (sprintf "%s/%s/%s" actor id msgType).ToLowerInvariant()
  
  static member MapGeneral(actor:ActorRef, msgType:Type) =
     { Path = HttpRoute.CreatePath(actor.Path.Type.Name, actor.Path.Id, msgType.Name)
       MsgType = msgType |> General
       Actor = actor }
  
  static member MapDU(actor:ActorRef, msgType) = 
     FSharpType.GetUnionCases(msgType) |> Array.map(fun c -> 
     { Path = HttpRoute.CreatePath(actor.Path.Type.Name, actor.Path.Id, c.Name)
       MsgType = msgType.DeclaringType |> DU
       Actor = actor })

  static member MapTyped(actor:TypedActorRef<'TActor>) = 
     typeof<'TActor>.GetMembers() |> Array.map(fun m ->
     { Path = HttpRoute.CreatePath(actor.Ref.Path.Type.Name, actor.Ref.Path.Id, m.Name)
       MsgType = typeof<Invocation> |> Typed
       Actor = actor.Ref })           


type Router(deserialize:string*Type -> obj, routes:IDictionary<string,HttpRoute>) =
   
   let getCaseName (path:string) = path
   let getMemberName (path:string) = path

   member this.Dispatch(key, msg) =      
      match routes.TryGetValue(key) with      
      | (true, route) ->
         let message = match route.MsgType with
                       | General msgType -> deserialize(msg, msgType)
                       
                       | DU msgType -> 
                          let caseName = msg |> getCaseName
                          let duMsg = MessageType.CreateDU(caseName, msg)
                          deserialize(duMsg, msgType)

                       | Typed msgType ->
                          let caseName = msg |> getMemberName
                          let typedMsg = MessageType.CreateTyped(caseName, msg)
                          deserialize(msg, msgType)

         route.Actor.Ask(message) |> Success
      | _  -> Failure "error"   

   static member Create deserialize (paths:HttpRoute seq) =       
      let dic = paths |> Seq.map(fun p -> (p.Path,p)) |> dict
      Router(deserialize, dic)

   static member MapHttpRoute(actor:ActorRef, msgType:Type) =
      match FSharpType.IsUnion(msgType) with   
      | true  -> HttpRoute.MapDU(actor, msgType)
      | false -> [| HttpRoute.MapGeneral(actor, msgType) |]

   static member MapHttpRoute(actor:TypedActorRef<'TActor>) = HttpRoute.MapTyped(actor)