module Demo

open System
open System.Reflection
open System.Threading.Tasks

open FSharp.Control.Tasks
open Orleankka
open Orleankka.Client
open Orleankka.FSharp
open Orleankka.Cluster
open Orleans.Hosting

open RealTimeCounter

// here we demonstrate a feature called "Reentrancy"
// more info you can find at: http://dotnet.github.io/orleans/Advanced-Concepts/Reentrant-Grains 

[<EntryPoint>]
let main argv =    
   
    printfn "Running demo. Booting cluster might take some time ...\n"

    let sb = new SiloHostBuilder()
    sb.AddAssembly(Assembly.GetExecutingAssembly())
    sb.ConfigureOrleankka() |> ignore

    use host = sb.Start().Result
    use client = host.Connect().Result

    let system = client.ActorSystem()
    let counter = ActorSystem.actorOf<ICounter>(system, "realtime-consistent-counter")

    let write = task {

        Console.ForegroundColor <- ConsoleColor.Red 
        printfn "\n send Increment message which should take 5 sec to finish. \n"
        do! counter <! Increment // this message is not reentrant which means blocking operation

        Console.ForegroundColor <- ConsoleColor.Red
        printfn "\n send Increment message which should take 5 sec to finish. \n"
        do! counter <! Increment 

        Console.ForegroundColor <- ConsoleColor.Red
        printfn "\n send Increment message which should take 5 sec to finish. \n"
        do! counter <! Increment 
    }

    let read = task {      
        let mutable count = 0
        while count < 3 do         
            let! result = counter <? GetCount // this message is reentrant which means not blocking operation
            count <- result
         
            Console.ForegroundColor <- ConsoleColor.Yellow
            printfn " current value is %d" count
            do! Task.Delay(TimeSpan.FromSeconds(0.5))

        Console.ForegroundColor <- ConsoleColor.Green
        printfn "\n job is finished."
    }
    
    Task.WaitAll(write, read)

    Console.ReadLine() |> ignore
    0 // return an integer exit code