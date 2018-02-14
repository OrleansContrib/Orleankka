module RealTimeCounter

open System
open FSharp.Control.Tasks
open Orleankka
open Orleankka.FSharp
open Orleans.Concurrency
open Orleans.CodeGeneration
open System.Threading.Tasks

type Message =
   | Increment
   | Decrement
   | GetCount

type ICounter = 
    inherit IActorGrain<Message>

[<MayInterleave("IsReentrant")>]
type Counter() =
    inherit FsActorGrain()

    let mutable count = 0

    interface ICounter
    override this.Receive(message, response) = task {
        match message with
        | :? Message as m -> 
            match m with
            | Increment -> do! Task.Delay(TimeSpan.FromSeconds(5.0)) // write to database a new value, IO bound blocking operation
                           count <- count + 1

            | Decrement -> do! Task.Delay(TimeSpan.FromSeconds(5.0)) // write to database a new value, IO bound blocking operation
                           count <- count - 1

            | GetCount -> response <? count // reentrant operation
        
        | _ -> response <? ActorGrain.Unhandled
    }

    static member IsReentrant(request:InvokeMethodRequest) =
        match request.Message() with
        | :? Message as m -> 
            match m with
            | GetCount -> true  // here we say that GetCount is reentrant message.
            | _        -> false
        | _ -> false