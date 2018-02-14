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

open Shop
open Account

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
    
    let t = task {
      
        let system = client.ActorSystem()
  
        let shop = ActorSystem.actorOf<IShop>(system, "amazon")
        let account = ActorSystem.actorOf<IAccount>(system, "john doe")

        let! stock = shop <? Stock
        printfn "Shop has %i items in stock \n" stock

        let! balance = account <? Balance
        printfn "Account balance is %i \n" balance

        printfn "Let's put 100$ on the account \n"
        do! account <! Deposit(100)      

        printfn "Let's put 5 items in stock \n"
        do! shop <! CheckIn(5)

        let! stock = shop <? Stock
        printfn "Now shop has %i items in stock \n" stock

        try
            printfn "Let's sell 100 items to user \n"
            do! shop <! Sell(account, 100)
        with :? InvalidOperationException as e -> printf "[Exception]: %s \n" e.Message

        printfn "Let's sell 2 items to user \n"
        do! shop <! Sell(account, 2)      

        let! stock = shop <? Stock
        printfn "Now shop has %i items in stock \n" stock

        let! balance = account <? Balance
        printfn "And account balance is %i \n" balance
    }

    t.Wait()

    Console.ReadKey() |> ignore
    0