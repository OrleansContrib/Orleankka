namespace Orleankka.FSharp

open FSharp.Control.Tasks
open System.Threading.Tasks   
open Orleankka

type Response() =
    member val internal result : obj = ActorGrain.Done :> obj with get,set
    static member (<?) (response:Response, result:obj) = response.result <- result

[<AbstractClass>]
type FsActorGrain =
   inherit ActorGrain

   new () = { inherit Orleankka.ActorGrain(); }

   new (id:string, runtime:IActorRuntime) = {
      inherit Orleankka.ActorGrain(id, runtime); 
   }

   abstract member Receive : message:obj * response:Response -> Task<unit>

   override this.Receive(message:obj) = task {        
      let response = Response()
      do! this.Receive(message, response)
      return response.result
   }
