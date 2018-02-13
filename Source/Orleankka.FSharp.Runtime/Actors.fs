namespace Orleankka.FSharp

open System.Threading.Tasks   
open Orleankka

type Response() =
    member this.result : obj = null

[<AbstractClass>]
type FSharpActorGrain =
   inherit ActorGrain

   new () = { inherit Orleankka.ActorGrain(); }

   new (id:string, runtime:IActorRuntime) = {
      inherit Orleankka.ActorGrain(id, runtime); 
   }

   abstract member Receive : message:obj * ctx:Response -> Task<unit>

   override this.Receive(message:obj) = task {        
      let ctx = Response()
      do! this.Receive(message, ctx)
      return ctx.result
   }

[<AutoOpen>]
module Actor =   

   let inline response (data:obj) = data
   let nothing = null
   

