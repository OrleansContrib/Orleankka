open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration

type Message = 
   | Greet of string
   | Hi   

type Greeter() = 
   inherit Actor<Message>()   

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

   use system = [|Assembly.GetExecutingAssembly()|]
                |> ActorSystem.createPlayground
                |> ActorSystem.start   

   let actor = ActorSystem.actorOf<Greeter>(system, "actor_id")

   let job() = task {
      do! actor <! Hi
      do! actor <! Greet "Yevhen"
      do! actor <! Greet "AntyaDev"      
   }
    
   Task.run(job) |> ignore
    
   Console.ReadLine() |> ignore 
   ActorSystem.stop(system)   
   0