
namespace Orleankka.FSharp

[<AutoOpen>]
module Actor =

   open System.Threading.Tasks
   open Orleankka
   open Orleankka.FSharp.Task   

   [<AbstractClass>]
   type Actor<'TMessage>() = 
      inherit Actor()
      
      let mutable _response = null
      
      let reply result = _response <- result

      abstract Receive : message:'TMessage -> reply:(obj -> unit) -> Task<unit>

      override this.OnReceive(msg : obj) = task {
         _response <- null
         do! this.Receive (msg :?> 'TMessage) reply
         return _response
      }
   
   let inline (<!) (actorRef:ActorRef) (message:obj) = actorRef.Ask(message) |> Task.map(ignore)
   let inline (<?) (actorRef:ActorRef) (message:obj) = actorRef.Ask<'TResponse>(message)
   let inline (<*) (ref:^ref) (message:obj) = (^ref : (member Notify : obj -> unit) (ref, message))