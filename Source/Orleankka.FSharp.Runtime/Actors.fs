namespace Orleankka.FSharp

open System.Threading.Tasks   
open Orleankka

[<AbstractClass>]
type ActorGrain<'TMsg> =
   inherit ActorGrain

   new () = { inherit Orleankka.ActorGrain(); }

   new (id:string, runtime:IActorRuntime) = {
      inherit Orleankka.ActorGrain(id, runtime); 
   }

   abstract Receive: message:'TMsg -> Task<obj>      

   override this.OnReceive(message:obj) = task {        
      match message with
      | :? 'TMsg as m -> return! this.Receive m                               
      | _             -> sprintf "Received unexpected message of type %s" (message.GetType().ToString()) |> failwith
                         return null
   }

   abstract member Activate: unit -> Task<unit>
   default this.Activate() = Task.completedTask
   override this.OnActivate() = this.Activate() :> Task


[<AutoOpen>]
module Actor =   

   let inline response (data:obj) = data
   let nothing = null
   

