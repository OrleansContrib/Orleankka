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

      abstract Receive: message:'TMessage * reply:(obj -> unit) -> Task<unit>
      
      abstract ReceiveAny: message:obj * reply:(obj -> unit) -> Task<unit>
      default this.ReceiveAny(message:obj, reply:obj -> unit) = Task.FromResult()

      override this.OnReceive(message:obj) = task {
         _response <- null
         match message with
         | :? 'TMessage as m -> do! this.Receive(m, reply)
                                return _response
         
         | _                 -> do! this.ReceiveAny(message, reply)
                                return _response
      }

      
   let inline (<?) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message))
   
   let inline (<!) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message)) |> Task.map(ignore)

   let inline (<*) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Notify : ^msg -> unit) (actorRef, message))