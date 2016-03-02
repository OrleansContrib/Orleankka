namespace Orleankka.FSharp

open System.Threading.Tasks
open Orleankka
open Orleankka.FSharp.Task

[<AutoOpen>]
module Actor =   

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

      
   let inline (<?) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message))
   
   let inline (<!) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message)) |> Task.map(ignore)

   let inline (<*) (actorRef:^ref) (message:^msg) = (^ref: (member Notify : ^msg -> unit) (actorRef, message))