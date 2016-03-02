namespace Orleankka.FSharp

open System.Threading.Tasks
open Orleankka
open Orleankka.FSharp.Task

module ActorRefExtensions =

   type Ref =
      static member Ask(ref:ActorRef, msg:obj) = ref.Ask<'TResponse>(msg)
      static member Ask(ref:ActorRef<'TActor>, msg:ActorMessage<'TActor>) = ref.Ask<'TResponse>(msg)     
      static member Tell(ref:ActorRef, msg:obj) = ref.Ask(msg) |> Task.map(ignore)
      static member Tell(ref:ActorRef<'TActor>, msg:ActorMessage<'TActor>) = ref.Ask(msg) |> Task.map(ignore)
      static member Notify(ref:ActorRef, msg:obj) = ref.Notify(msg)
      static member Notify(ref:ActorRef<'TActor>, msg:ActorMessage<'TActor>) = ref.Notify(msg)

[<AutoOpen>]
module Actor =

   open ActorRefExtensions

   [<AbstractClass>]
   type Actor<'TMessage>() = 
      inherit Actor()
      
      let mutable _response = null
      
      let reply result = _response <- result

      abstract Receive: message:'TMessage * reply:(obj -> unit) -> Task<unit>
      
      abstract ReceiveUntyped: message:obj * reply:(obj -> unit) -> Task<unit>
      default this.ReceiveUntyped(message:obj, reply:obj -> unit) = Task.FromResult()

      override this.OnReceive(message:obj) = task {
         _response <- null
         match message with
         | :? 'TMessage as m -> do! this.Receive(m, reply)
                                return _response
         
         | _                 -> do! this.ReceiveUntyped(message, reply)
                                return _response
      }

   [<AbstractClass>]
   type Actor<'TMessage1, 'TMessage2>() = 
      inherit Actor()
      
      let mutable _response = null
      
      let reply result = _response <- result

      abstract Receive: message:'TMessage1 * reply:(obj -> unit) -> Task<unit>
      abstract Receive: message:'TMessage2 * reply:(obj -> unit) -> Task<unit>
      
      abstract ReceiveUntyped: message:obj * reply:(obj -> unit) -> Task<unit>
      default this.ReceiveUntyped(message:obj, reply:obj -> unit) = Task.FromResult()

      override this.OnReceive(message:obj) = task {
         _response <- null
         match message with
         | :? 'TMessage1 as m -> do! this.Receive(m, reply)
                                 return _response
                                           
         | :? 'TMessage2 as m -> do! this.Receive(m, reply)
                                 return _response         

         | _                  -> do! this.ReceiveUntyped(message, reply)
                                 return _response
      }

   [<AbstractClass>]
   type Actor<'TMessage1, 'TMessage2, 'TMessage3>() = 
      inherit Actor()
      
      let mutable _response = null
      
      let reply result = _response <- result

      abstract Receive: message:'TMessage1 * reply:(obj -> unit) -> Task<unit>
      abstract Receive: message:'TMessage2 * reply:(obj -> unit) -> Task<unit>
      abstract Receive: message:'TMessage3 * reply:(obj -> unit) -> Task<unit>
      
      abstract ReceiveUntyped: message:obj * reply:(obj -> unit) -> Task<unit>
      default this.ReceiveUntyped(message:obj, reply:obj -> unit) = Task.FromResult()

      override this.OnReceive(message:obj) = task {
         _response <- null
         match message with
         | :? 'TMessage1 as m -> do! this.Receive(m, reply)
                                 return _response
                                           
         | :? 'TMessage2 as m -> do! this.Receive(m, reply)
                                 return _response
         
         | :? 'TMessage3 as m -> do! this.Receive(m, reply)
                                 return _response         

         | _                  -> do! this.ReceiveUntyped(message, reply)
                                 return _response
      }

   [<AbstractClass>]
   type Actor<'TMessage1, 'TMessage2, 'TMessage3, 'TMessage4>() = 
      inherit Actor()
      
      let mutable _response = null
      
      let reply result = _response <- result

      abstract Receive: message:'TMessage1 * reply:(obj -> unit) -> Task<unit>
      abstract Receive: message:'TMessage2 * reply:(obj -> unit) -> Task<unit>
      abstract Receive: message:'TMessage3 * reply:(obj -> unit) -> Task<unit>
      abstract Receive: message:'TMessage4 * reply:(obj -> unit) -> Task<unit>
      
      abstract ReceiveUntyped: message:obj * reply:(obj -> unit) -> Task<unit>
      default this.ReceiveUntyped(message:obj, reply:obj -> unit) = Task.FromResult()

      override this.OnReceive(message:obj) = task {
         _response <- null
         match message with
         | :? 'TMessage1 as m -> do! this.Receive(m, reply)
                                 return _response
                                           
         | :? 'TMessage2 as m -> do! this.Receive(m, reply)
                                 return _response
         
         | :? 'TMessage3 as m -> do! this.Receive(m, reply)
                                 return _response
         
         | :? 'TMessage4 as m -> do! this.Receive(m, reply)
                                 return _response         

         | _                  -> do! this.ReceiveUntyped(message, reply)
                                 return _response
      }

      
   let inline (<?) actorRef message = Ref.Ask(actorRef, message)      
   let inline (<!) actorRef message = Ref.Tell(actorRef, message)   
   let inline (<*) actorRef message = Ref.Notify(actorRef, message)