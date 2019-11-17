namespace Orleankka.FSharp

open Orleankka
open Orleankka.FSharp

[<RequireQualifiedAccess>]
module ActorSystem =       

   let inline actorOf<'TActor when 'TActor :> IActor> (system:IActorSystem, actorId) =
      let actorPath = ActorPath.For<'TActor>(actorId) 
      system.ActorOf(actorPath) |> FSharp.ActorRef<obj>

   let inline actorOfPath(system:IActorSystem) path =
      system.ActorOf(path) |> FSharp.ActorRef<obj>

   let inline typedActorOf<'TActor when 'TActor :> IActor> (system:IActorSystem, actorId) =
      let actorPath = ActorPath.For<'TActor>(actorId) 
      system.ActorOf(actorPath) |> FSharp.ActorRef<'TActor>

   let inline streamOf (system:IActorSystem) provider streamId = 
      system.StreamOf(provider, streamId) |> FSharp.StreamRef<'TMsg>