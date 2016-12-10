namespace FSharp.Runtime
open System.Reflection
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.FSharp
open Orleankka.Playground
open Orleankka.Cluster
type Class1() = 
    member this.X = "F#"
