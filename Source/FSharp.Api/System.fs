
module Orleankka.FSharp.System

open Orleankka
open Orleankka.Cluster
open Orleankka.Client
open Orleankka.Embedded
open Orleankka.Playground

let inline playgroundActorSystem () = ActorSystem.Configure().Playground()
let inline initClusterActorSystem config = ActorSystem.Configure().Cluster().From(config)
let inline initClientActorSystem config = ActorSystem.Configure().Client().From(config)

let inline register data silo =
   (^silo : (member Register : ^data -> ^silo) (silo, data))

let inline run<'T when 'T :> Bootstrapper> properties (silo : EmbeddedConfigurator) =
   silo.Run<'T>(properties)

let inline start silo = 
   (^silo : (member Done : unit -> IActorSystem) (silo))