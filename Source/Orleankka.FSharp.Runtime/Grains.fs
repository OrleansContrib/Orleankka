namespace Orleankka.FSharp

open Orleankka

[<AutoOpen>]
module ReceiveResult =   

   let inline some (data:obj) = data
   let none() = ActorGrain.Done :> obj
   let unhandled() = some(ActorGrain.Unhandled)