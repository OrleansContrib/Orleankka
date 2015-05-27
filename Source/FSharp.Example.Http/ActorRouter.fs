module ActorRouter

open Orleankka
open System
open System.Collections.Generic
open Newtonsoft.Json

type Result<'TSuccess,'TFailure> = 
   | Success of 'TSuccess
   | Failure of 'TFailure

type MediaType =
   | VndActor
   | VndTypedActor
   with 
   static member VndActorJson = "orleankka/vnd.actor+json"
   static member VndTypedActorJson = "orleankka/vnd.typed.actor+json"

let private createKey (actor, id) =
   (actor + "/" + id).ToLowerInvariant()


type Router(deserialize:string*Type -> obj, paths:IDictionary<string,Type*ActorRef>) =
   
   member this.Dispatch(actor, id, msg) =
      let key = createKey(actor, id) 
      match paths.TryGetValue(key) with
      
      | true, (msgType,actorRef) ->          
         let message = deserialize(msg, msgType)
         Success (actorRef.Ask(message))
      
      | _  -> Failure "error"


let mapToPath (actor:ActorRef, message:Type) =
   createKey(actor.Path.Type.Name, actor.Path.Id), (message, actor)

let create deserialize paths = 
   Router(deserialize, paths |> dict)