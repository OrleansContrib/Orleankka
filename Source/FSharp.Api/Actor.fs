namespace Orleankka.FSharp

[<AutoOpen>]
module Actor =   

   open System.Threading.Tasks
   open Orleankka
   open Orleankka.FSharp.Task

   [<AbstractClass>]
   type Actor<'TMessage>() = 
      inherit Actor()

      abstract Receive: message:'TMessage * reply:(obj -> unit) -> Task<unit>
      
      abstract ReceiveAny: message:obj * reply:(obj -> unit) -> Task<unit>
      default this.ReceiveAny(message:obj, reply:obj -> unit) = 
        match message with
            | :? 'TMessage as m -> this.Receive(m, reply)
            | _                 -> sprintf "Received unexpected message of type %s" (message.GetType().ToString()) |>  failwith 

      override this.OnReceive(message:obj) = task {
        let mutable response = null
        let reply result = response <- result
        do! this.ReceiveAny(message, reply)
        return response
      }

      
   let inline (<?) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message))
   
   let inline (<!) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message)) |> Task.map(ignore)

   let inline (<*) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Notify : ^msg -> unit) (actorRef, message))