
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

      abstract Receive: message:'TMessage -> reply:(obj -> unit) -> Task<unit>
      
      abstract UntypedReceive: message:obj -> reply:(obj -> unit) -> Task<unit>
      default this.UntypedReceive (message:obj) (reply:obj -> unit) = Task.FromResult()

      override this.OnReceive(message:obj) = task {
         _response <- null
         match message with
         | :? 'TMessage as m -> do! this.Receive m reply
                                return _response
         
         | _                 -> do! this.UntypedReceive message reply
                                return _response
      }
   
   let inline (<!) (actorRef:ActorRef) (message:obj) = actorRef.Ask(message) |> Task.map(ignore)
   let inline (<?) (actorRef:ActorRef) (message:obj) = actorRef.Ask<'TResponse>(message)
   let inline (<*) (ref:^ref) (message:obj) = (^ref : (member Notify : obj -> unit) (ref, message))