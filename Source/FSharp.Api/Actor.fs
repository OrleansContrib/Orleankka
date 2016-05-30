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

      abstract Receive: message:'TMessage -> Task<obj>      

      override this.OnReceive(message:obj) = task {        
        match message with
        | :? 'TMessage as m -> return! this.Receive m                               
        | _                 -> sprintf "Received unexpected message of type %s" (message.GetType().ToString()) |> failwith
                               return null
      }


   let inline response (data:obj) = data

   let inline (<?) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message))
   
   let inline (<!) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Ask : ^msg -> Task<'TRresponse>) (actorRef, message)) |> Task.map(ignore)

   let inline (<*) (actorRef:^ref) (message:^msg) = 
      (^ref: (member Notify : ^msg -> unit) (actorRef, message))


[<AutoOpen>]
module FSharpStream =
   
   open Orleankka.FSharp

   type StreamRef(ref:Orleankka.StreamRef) =
      inherit Orleankka.StreamRef(ref.Path)

      member this.Push(item:obj) =
         this.Push(item) |> awaitTask


[<AutoOpen>]
module FSharpActorSystem =
   
   open Orleankka   

   type IActorSystem with

      member this.StreamOf(provider:string, id:string) =
         StreamPath.From(provider, id)
            |> this.StreamOf
            |> FSharpStream.StreamRef

      member this.StreamOf(path:StreamPath) =
         path |> this.StreamOf |> FSharpStream.StreamRef