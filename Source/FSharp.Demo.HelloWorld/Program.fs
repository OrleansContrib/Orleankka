module Demo

open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.Client
open Orleankka.Cluster
open Orleans
open Orleans.Hosting
open Orleans.Runtime.Configuration

type Message = 
   | Greet of string
   | Hi

type IGreeter = 
   inherit IActorGrain

type Greeter() = 
   inherit FSharpActorGrain()
   interface IGreeter

   override this.Receive(message, _) = task {
      match message with
        | :? Message as m -> 
            match m with
            | Greet who -> printfn "Hello %s" who
            | Hi        -> printfn "Hello from F#!"
        | _ -> ignore()
   }

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

    let host = sb.Build()
    host.StartAsync().Wait()

    let cc = ClientConfiguration.LocalhostSilo()
    cc.AddSimpleMessageStreamProvider("sms")

    let cb = new ClientBuilder()
    cb.UseConfiguration(cc) |> ignore
    cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore) |> ignore
    cb.ConfigureOrleankka() |> ignore

    let client = cb.Build()
    client.Connect().Wait()
   
    let system = client.ActorSystem()
   
    let job() = task {
        let actor = ActorSystem.actorOf<IGreeter>(system, "actor_id")
      
        do! actor <! Hi
        do! actor <! Greet "Yevhen"
        do! actor <! Greet "AntyaDev"      
    }
    
    Task.run(job) |> ignore
    
    Console.ReadLine() |> ignore 
    0