open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
//open Orleankka.FSharp.Configuration

type Message = 
   | Greet of string
   | Hi   

open ActorExpression

// let myActors = new System.Collections.Generic.List<Configurations.ActorConfiguration>()

actor {
   typeName "defined"
   body (fun ()-> 
      fun msg -> 
         msg.GetType().FullName |> printfn "received %s" 
         1 |> response

)} |> ActorRegister.add

actor {
   typeName "counter"
   body (fun () ->   
      let mutable state = 0     
      fun msg ->
         state <- state+1
         printfn "state is %d" state
         state |> response

)} |> ActorRegister.add

[<EntryPoint>]
let main argv = 

   printfn "Running demo. Booting cluster might take some time ...\n"

   let system = ActorExpression.RegistrationExample.createPlayground() 
   system.Start()
   let ref = system.ActorOf("defined","@")

   let statefull = system.ActorOf("counter","@")
   let statefull2 = system.ActorOf("counter","2")
   let job() = task {
      do! ref <! Hi
      do! statefull <! ""
      do! statefull <! ""
      do! statefull <! ""
      do! statefull <! ""

      do! statefull2 <! ""
      do! statefull2 <! ""
      do! statefull2 <! ""
   }
    
   Task.run(job) |> ignore
    
   Console.ReadLine() |> ignore    
   0