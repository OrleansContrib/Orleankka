module ActorRouter

open Orleankka
open Rop

open System
open System.Collections.Generic
open Newtonsoft.Json

let private createKey (actor:string, id:string) =
   (actor + "/" + id).ToLowerInvariant()


type Router(deserialize:string*Type -> obj, paths:IDictionary<string,Type*ActorRef>) =
   
   member this.Dispatch(actor:string, id:string, msg:string) =
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