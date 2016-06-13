namespace Orleankka.FSharp

[<AutoOpen>]
module Actor = 
   open System.Threading.Tasks   
   open Orleankka  
   open Orleankka.CSharp  

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
   let nothing = null


[<RequireQualifiedAccess>]
module ClientObserver =
   open Orleankka

   let inline create () = ClientObserver.Create()

[<AutoOpen>]
module ActorRef =
   open Orleankka   

   let inline getPath (ref:ActorRef) = ref.Path

   let inline notify (ref:ActorRef) (message:obj) = ref.Notify message

   let inline tell (ref:ActorRef) (message:obj) = ref.Tell(message) |> awaitTask

   let inline ask<'TResponse> (ref:ActorRef) (message:obj) = ref.Ask<'TResponse>(message)
   
   
[<AutoOpen>]
module StreamRef = 
   open System.Threading.Tasks
   open Orleankka
   open Orleankka.CSharp   

   let inline getPath (ref:StreamRef) = ref.Path

   let inline push (ref:StreamRef) (message:obj) = ref.Push(message) |> Task.awaitTask

   let inline subscribe<'TMessage> (ref:StreamRef) (callback:'TMessage -> unit) = ref.Subscribe<'TMessage>(callback)

   let inline subscribeF<'TMessage> (ref:StreamRef) (callback:'TMessage -> unit) (filter:StreamFilter) = ref.Subscribe<'TMessage>(callback, filter)   

   let inline subscribeActor (ref:StreamRef) (actor:Actor) = ref.Subscribe(actor) |> Task.awaitTask

   let inline subscribeActorF (ref:StreamRef) (actor:Actor) (filter:StreamFilter) = ref.Subscribe(actor, filter) |> Task.awaitTask
   
   
[<AutoOpen>]
module StreamSubscription =      
   open Orleankka
   
   let inline unsubscribe (sb:StreamSubscription) = sb.Unsubscribe() |> Task.awaitTask


[<AutoOpen>]
module Operators =
   open Orleankka
   
   type Api = Api with      
      static member (<!) (api:Api, actorRef:ActorRef)   = fun (msg:obj) -> actorRef.Tell(msg) |> Task.awaitTask
      static member (<!) (api:Api, streamRef:StreamRef) = fun (msg:obj) -> streamRef.Push(msg) |> Task.awaitTask

   let inline (<*) (x:^T) (message:obj) = (^T: (member Notify: obj -> unit) (x, message))   
   
   let inline (<!) (x:'T) (message:obj) = Api <! x <| message

   let inline (<?) (x:ActorRef) (message:obj) = x.Ask(message)