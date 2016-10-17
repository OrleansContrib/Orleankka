open System
open System.Reflection

open Orleankka
open Orleankka.CSharp
open Orleankka.FSharp
//open Orleankka.FSharp.Configuration

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

   let system = ActorExpression.RegistrationExample.createPlayground() 
//   system.Start()               
//   let actor = system.ActorOf<Greeter>(Guid.NewGuid().ToString())
//
//   let job() = task {
//      do! actor <! Hi
//      do! actor <! Greet "Yevhen"
//      do! actor <! Greet "AntyaDev"
//   }
    
//   Task.run(job) |> ignore
    
   Console.ReadLine() |> ignore    
   0