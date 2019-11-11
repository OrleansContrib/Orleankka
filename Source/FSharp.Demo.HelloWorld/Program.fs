module Demo

open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.Playground

type Message = 
   | Greet of string
   | Hi   

type IGreeter = 
   inherit IActor

type Greeter() = 
   inherit Actor<Message>()   
   interface IGreeter

   override this.Receive message = task {
      match message with
      | Greet who -> printfn "Hello %s" who
                     return nothing
      
      | Hi        -> printfn "Hello from F#!"
                     return nothing           
   }

[<EntryPoint>]
let main argv = 

   printfn "Running demo. Booting cluster might take some time ...\n"
   
   use system = 
    ActorSystem
     .Configure()
     .Playground()
     .Assemblies([|Assembly.GetExecutingAssembly()|])
     .Done()
   
   let job() = task {
      do! Task.awaitTask(system.Start())
      
      let actor = ActorSystem.actorOf<IGreeter>(system, "actor_id")
      do! actor <! Hi
      do! actor <! Greet "Yevhen"
      do! actor <! Greet "AntyaDev"      
   }
    
   Task.run(job) |> ignore
    
   Console.ReadLine() |> ignore 
   0