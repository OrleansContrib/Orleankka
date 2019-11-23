module RealTimeCounter

open System
open System.Threading.Tasks
open FSharp.Control.Tasks
open Orleankka
open Orleankka.FSharp
open Orleans.Concurrency
open Orleans.CodeGeneration

type CounterMessage =
   | Increment
   | Decrement
   | GetCount

type ICounter = 
    inherit IActorGrain<CounterMessage>

[<MayInterleave("IsReentrant")>]
type Counter() =
    inherit ActorGrain()

    let mutable count = 0

    interface ICounter
    override this.Receive(message) = task {
        match message with
        | :? CounterMessage as m -> 
            match m with
            | Increment -> do! Task.Delay(TimeSpan.FromSeconds(5.0)) // write to database a new value, IO bound blocking operation
                           count <- count + 1
                           return none()

            | Decrement -> do! Task.Delay(TimeSpan.FromSeconds(5.0)) // write to database a new value, IO bound blocking operation
                           count <- count - 1
                           return none()

            | GetCount -> return some(count) // reentrant operation
        
        | _ -> return unhandled()
    }

    static member IsReentrant(request:InvokeMethodRequest) = 
        match request.Message() with
        | :? CounterMessage as m -> 
            match m with
            | GetCount  -> true 
            | _ -> false
        | _ -> false