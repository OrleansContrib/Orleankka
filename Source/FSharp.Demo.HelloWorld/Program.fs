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
   system.Start()
   let ref = system.ActorOf("testActor","@")

   let job() = task {
      do! ref <! Hi
   }
    
   Task.run(job) |> ignore
    
   Console.ReadLine() |> ignore    
   0