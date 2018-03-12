module Demo

open System
open System.Reflection

open FSharp.Control.Tasks
open Orleankka
open Orleankka.Client
open Orleankka.FSharp
open Orleankka.Cluster
open Orleans.Hosting

type GreeterMessage = 
   | Greet of string
   | Hi

type IGreeter = 
   inherit IActorGrain<GreeterMessage>

type Greeter() = 
   inherit ActorGrain()
   interface IGreeter

   override this.Receive(message) = task {
      match message with
        | :? GreeterMessage as m -> 
            match m with
            | Greet who -> printfn "Hello %s" who
                           return none()

            | Hi        -> printfn "Hello from F#!"
                           return none()

        |_ -> return unhandled()
   }

[<EntryPoint>]
let main argv = 

    printfn "Running demo. Booting cluster might take some time ...\n"

    let sb = new SiloHostBuilder()
    sb.AddAssembly(Assembly.GetExecutingAssembly())
    sb.ConfigureOrleankka() |> ignore

    use host = sb.Start().Result
    use client = host.Connect().Result
   
    let t = task {

        let system = client.ActorSystem()
        let actor = ActorSystem.typedActorOf<IGreeter, GreeterMessage>(system, "good-citizen")
      
        do! actor <! Hi
        do! actor <! Greet "Yevhen"
        do! actor <! Greet "World"      
    }
    t.Wait()
    
    Console.ReadKey() |> ignore 
    0