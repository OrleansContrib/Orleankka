open System
open System.Reflection
open Orleans
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.System

type Message = 
   | Greet of string
   | Hi

type Greeter() as this = 
   inherit FunActor()   

   do 
      this.Receive(fun message -> task {      
        match message with
        | Greet who -> printfn "Hello %s" who
        | Hi -> printfn "Hello from F#!"     
    })   

[<EntryPoint>]
let main argv = 

    printfn "Running demo. Booting cluster might take some time ...\n"

    let assembly = Assembly.GetExecutingAssembly()
   
    use system = playgroundActorSystem()
                |> register [|assembly|]
                |> start
                  
    let actor = system.ActorOf<Greeter>(Guid.NewGuid().ToString())

    let t = task {
        do! actor <? Hi
        do! actor <? Greet "Yevhen"
        do! actor <? Greet "AntyaDev"
    } 
        
    t.Wait()     
    Console.ReadLine() |> ignore

    printfn "%A" argv
    0