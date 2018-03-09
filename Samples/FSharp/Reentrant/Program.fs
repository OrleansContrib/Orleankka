module Demo

open System
open System.Reflection

open FSharp.Control.Tasks
open Orleankka.FSharp
open Orleankka.Client
open Orleankka.Cluster
open Orleans
open Orleans.Hosting
open Orleans.Runtime.Configuration

open RealTimeCounter
open System.Threading.Tasks

// here we demonstrate a feature called "Reentrancy"
// more info you can find at: http://dotnet.github.io/orleans/Advanced-Concepts/Reentrant-Grains 

[<EntryPoint>]
let main argv =    
   
    printfn "Running demo. Booting cluster might take some time ...\n"

    let sc = ClusterConfiguration.LocalhostPrimarySilo()
                  
    sc.AddMemoryStorageProvider()
    sc.AddMemoryStorageProvider("PubSubStore")
    sc.AddSimpleMessageStreamProvider("sms")

    let sb = new SiloHostBuilder()
    sb.UseConfiguration(sc) |> ignore
    sb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore) |> ignore
    sb.ConfigureOrleankka() |> ignore

    use host = sb.Build()
    host.StartAsync().Wait()

    let cc = ClientConfiguration.LocalhostSilo()
    cc.AddSimpleMessageStreamProvider("sms")

    let cb = new ClientBuilder()
    cb.UseConfiguration(cc) |> ignore
    cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore) |> ignore
    cb.ConfigureOrleankka() |> ignore

    use client = cb.Build()
    client.Connect().Wait()

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