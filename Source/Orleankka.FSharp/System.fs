namespace Orleankka.FSharp

open Orleankka
open Orleankka.FSharp

[<RequireQualifiedAccess>]
module ActorSystem =       

   let inline actorOf<'TActor when 'TActor :> IActorGrain> (system:IActorSystem, actorId) =
      let actorPath = ActorPath.For(typeof<'TActor>, actorId) 
      system.ActorOf(actorPath) |> FSharp.ActorRef<obj>

   let inline actorOfPath(system:IActorSystem, path) =
      system.ActorOf(path) |> FSharp.ActorRef<obj>

   let inline typedActorOf<'TActor, 'TMsg when 'TActor :> IActorGrain<'TMsg>> (system:IActorSystem, actorId) =
      let actorPath = ActorPath.For(typeof<'TActor>, actorId)
      system.ActorOf(actorPath) |> FSharp.ActorRef<'TMsg>

   let inline streamOf (system:IActorSystem, provider, streamId) = 
      system.StreamOf(provider, streamId) |> FSharp.StreamRef<'TMsg>