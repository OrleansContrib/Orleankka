
module Orleankka.FSharp.System

open Orleankka
open Orleankka.Cluster
open Orleankka.Client
open Orleankka.Embedded
open Orleankka.Playground

let inline playgroundConfigurator () = ActorSystem.Configure().Playground()
let inline clusterConfigurator config = ActorSystem.Configure().Cluster().From(config)
let inline clientConfigurator config = ActorSystem.Configure().Client().From(config)

let inline register data silo =
   (^silo : (member Register : ^data -> ^configurator) (silo, data))

let inline run<'T when 'T :> Bootstrapper> properties (silo : EmbeddedConfigurator) =
   silo.Run<'T>(properties)

let inline start silo = 
   (^silo : (member Done : unit -> IActorSystem) (silo))