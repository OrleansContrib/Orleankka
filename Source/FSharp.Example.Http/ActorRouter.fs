module ActorRouter

open Orleankka
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

type RouterPath = {
   Path : string
   MsgType : Type
   Actor : ActorRef
}

type Router(deserialize:string*Type -> obj, paths:IDictionary<string,RouterPath>) =
   
   member this.Dispatch(key, msg) =      
      match paths.TryGetValue(key) with
      
      | (true,routerPath) ->          
         let message = deserialize(msg, routerPath.MsgType)
         Success (routerPath.Actor.Ask(message))      
      
      | _  -> Failure "error"
      

let createPath (actor, id, msgType) =
   (sprintf "%s/%s/%s" actor id msgType).ToLowerInvariant()

let mapToPath (actor:ActorRef, msgType:Type) =
   { Path = createPath(actor.Path.Type.Name, actor.Path.Id, msgType.Name)
     MsgType = msgType
     Actor = actor }   

let mapDUToPath (actor:ActorRef, msgType) =   
   FSharpType.GetUnionCases(msgType) |> Array.map(fun c -> 
   { Path = createPath(actor.Path.Type.Name, actor.Path.Id, c.Name)
     MsgType = msgType.DeclaringType
     Actor = actor })
      
let mapToPaths (actor:ActorRef, msgType:Type) =   
   match FSharpType.IsUnion(msgType) with   
   | true  -> mapDUToPath(actor, msgType)
   | false -> [| mapToPath(actor,msgType) |]
   
let create deserialize (paths:RouterPath seq) =       
   let dic = paths |> Seq.map(fun p -> (p.Path,p)) |> dict
   Router(deserialize, dic)