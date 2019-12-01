namespace Orleankka.FSharp

open Orleans
open Orleankka
open Orleankka.FSharp

[<RequireQualifiedAccess>]
module ActorSystem =       

   let inline actorOf<'TActor when 'TActor :> IActorGrain and 'TActor :> IGrainWithStringKey> (system:IActorSystem, actorId) =
      system.ActorOf<'TActor>(actorId) |> FSharp.ActorRef<obj>

   let inline actorOfPath(system:IActorSystem, path) =
      system.ActorOf(path) |> FSharp.ActorRef<obj>

   let inline typedActorOf<'TActor, 'TMsg when 'TActor :> IActorGrain<'TMsg> and 'TActor :> IGrainWithStringKey> (system:IActorSystem, actorId) =
     system.ActorOf<'TActor>(actorId) |> FSharp.ActorRef<'TMsg>

   let inline streamOf (system:IActorSystem, provider, streamId) = 
      system.StreamOf(provider, streamId) |> FSharp.StreamRef<'TMsg>