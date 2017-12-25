namespace Orleankka.FSharp

open Orleankka

type ActorRef<'TMsg>(ref:ActorRef) =    
   member this.Path = ref.Path
   member this.Tell(message:'TMsg) = ref.Tell(message) |> Task.awaitTask 
   member this.Ask(message:'TMsg) = ref.Ask<'TResponse>(message)
   member this.Notify(message:'TMsg) = ref.Notify(message)
   
   override this.Equals(other:obj) = 
      match other with
      | :? ActorRef<'TMsg> as ar -> this.Path = ar.Path
      | _ -> false
       
   override this.GetHashCode() = ref.GetHashCode()

   static member (<!) (ref:ActorRef<'TMsg>, message:'TMsg) = ref.Tell(message)
   static member (<?) (ref:ActorRef<'TMsg>, message:'TMsg) = ref.Ask<'TResponse>(message)
   static member (<*) (ref:ActorRef<'TMsg>, message:'TMsg) = ref.Notify(message)

type StreamRef<'TMsg>(ref:StreamRef) = 
   member this.Path = ref.Path
   member this.Push(item:'TMsg) = ref.Push(item) |> Task.awaitTask
   member this.Subscribe(callback:'TMsg -> unit) = ref.Subscribe<'TMsg>(callback)
   member this.Subscribe(callback:'TMsg -> unit, filter:StreamFilter) = ref.Subscribe<'TMsg>(callback, filter)   

   override this.Equals(other:obj) = 
      match other with
      | :? StreamRef<'TMsg> as sr -> this.Path = sr.Path
      | _ -> false
       
   override this.GetHashCode() = ref.GetHashCode()

   static member (<!) (ref:StreamRef<'TMsg>, item:'TMsg) = ref.Push(item)
