
[<AutoOpen>]

module Orleankka.FSharp.System

open Orleankka
open Orleankka.Cluster
open Orleankka.Embedded
open Orleankka.Playground

let inline playgroundActorSystem () = ActorSystem.Configure().Playground()

let inline register data silo =
   (^silo : (member Register : ^data -> EmbeddedConfigurator) (silo, data))

let inline run<'T when 'T :> Bootstrapper> properties (silo : EmbeddedConfigurator) =
   silo.Run<'T>(properties)

let inline start (cfg : EmbeddedConfigurator) = cfg.Done()