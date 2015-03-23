open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.System

type Message = 
   | Greet of string
   | Hi

type Greeter() = 
   inherit Actor<Message>()   

   override this.Receive message reply = task {
      match message with
      | Greet who -> printfn "Hello %s" who
      | Hi -> printfn "Hello from F#!"           
   }

[<EntryPoint>]
let main argv = 

    printfn "Running demo. Booting cluster might take some time ...\n"

    let assembly = Assembly.GetExecutingAssembly()
   
    use system = playgroundConfigurator()
                 |> register [|assembly|]
                 |> start
                  
    let actor = system.ActorOf<Greeter>(Guid.NewGuid().ToString())

    task {
      do! actor <! Hi
      do! actor <! Greet "Yevhen"
      do! actor <! Greet "AntyaDev"
    } 
    |> Task.wait
    
    Console.ReadLine() |> ignore

    printfn "%A" argv
    0