namespace Orleankka.FSharp

[<AutoOpen>]
module Actor =   
   
   open System
   open System.Threading.Tasks
   open Orleankka
   open Orleankka.FSharp.Task

   [<AbstractClass>]
   type Actor<'TMessage>() = 
      inherit Actor()

      abstract Receive: message:'TMessage -> reply:(obj -> unit) -> Task<unit>      

      override this.OnReceive(message:obj) = task {
        let mutable response = null
        let reply result = response <- result
        match message with
        | :? 'TMessage as m -> do! this.Receive m reply
                               return response        
        | _                 -> sprintf "Received unexpected message of type %s" (message.GetType().ToString()) |> failwith
                               return response
      }

      
   let inline (<?) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message))
   
   let inline (<!) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message)) |> Task.map(ignore)

   let inline (<*) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Notify : ^msg -> unit) (actorRef, message))