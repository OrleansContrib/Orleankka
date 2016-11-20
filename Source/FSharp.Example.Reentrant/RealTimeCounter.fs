module RealTimeCounter

open System
open Orleankka
open Orleankka.CSharp
open Orleankka.FSharp

type Message =
   | Increment
   | Decrement
   | GetCount

[<Reentrant("IsReentrant")>]
type Counter() =
   inherit Actor<Message>()

   let mutable count = 0

   override this.Receive msg = task {
      match msg with
      | Increment -> do! Task.delay(TimeSpan.FromSeconds(5.0)) // write to database a new value, IO bound blocking operation
                     count <- count + 1
                     return nothing

      | Decrement -> do! Task.delay(TimeSpan.FromSeconds(5.0)) // write to database a new value, IO bound blocking operation
                     count <- count - 1
                     return nothing

      | GetCount -> return response(count) // reentrant operation, is not blocking.
   }

   // this method will be invoked by Orleankka runtime in order to distinguish message on reentrancy,
   // basically it's just a filter method.
   static member IsReentrant(msg:obj) =
      match msg with
      | :? Message as m -> match m with
                           | GetCount -> true  // here we say that GetCount is reentrant message.
                           | _        -> false
      | _ -> false