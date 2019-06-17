namespace Contracts
open Orleankka
open Orleankka.FSharp
open System.Net
module Say =

  type HelloMessages =
    | Hi
    | Hello of string
    | Bue

  type IHello =
    inherit IActorGrain<HelloMessages>
