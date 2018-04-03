This tutorial is intended to give an introduction to using Orleankka by creating a simple greeter actor using F#.

## Set up your project

Start Visual Studio and create a new C# Console Application and reference the following packages:

```PM
PM> Install-Package Orleankka.Runtime
PM> Install-Package Orleankka.FSharp.Runtime
PM> Install-Package Microsoft.Orleans.Server
PM> Install-Package FSharp.Control.Tasks
```
This will install all client and server-side packages required for demo app.

## Create your first actor

First, we need to create a message type that our actor will respond to:

```fsharp
module Demo

type GreeterMessage = 
   | Greet of string
   | Hi
```

Once we have the message type, we can create our actor:

```fsharp
module Demo

open FSharp.Control.Tasks  // task CE from Giraffe
open Orleankka             // base types of Orleankka
open Orleankka.FSharp

type GreeterMessage = 
   | Greet of string
   | Hi

// Create custom actor interface and implement IActorGrain
type IGreeter = 
   inherit IActorGrain<GreeterMessage>

// Create actor class by inheriting from ActorGrain and implementing custom actor interface
type Greeter() = 
   inherit ActorGrain()
   interface IGreeter

   // Implement receive function using pattern matching
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
```

Now it's time to consume our actor. We need to configure Orleans and then register Orleankka. We'll be using simple localhost configuration suitable for demos.

```fsharp
open System
open System.Net
open System.Reflection

open FSharp.Control.Tasks
open Orleans
open Orleans.Hosting
open Orleans.Configuration
open Orleans.Runtime
open Orleankka
open Orleankka.Cluster
open Orleankka.Client
open Orleankka.FSharp

[<EntryPoint>]
let main argv = 

    let DemoClusterId = "localhost-demo"
    let LocalhostSiloPort = 11111
    let LocalhostGatewayPort = 30000
    let LocalhostSiloAddress = IPAddress.Loopback

    printfn "Running demo. Booting cluster might take some time ...\n"

    // configure localhost silo
    let sb = new SiloHostBuilder()
    sb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId) |> ignore
    sb.UseDevelopmentClustering(fun (options:DevelopmentClusterMembershipOptions) -> options.PrimarySiloEndpoint <- IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort)) |> ignore
    sb.ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort) |> ignore
    
    // register assembly containing your custom actor grain interfaces
    sb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore) |> ignore

    // register Orleankka extension
    sb.ConfigureOrleankka() |> ignore
  
    // configure localhost silo client
    let cb = new ClientBuilder()
    cb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId) |> ignore
    cb.UseStaticClustering(fun (options:StaticGatewayListProviderOptions) -> options.Gateways.Add(IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri())) |> ignore

    // register assembly containing your custom actor grain interfaces
    cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore) |> ignore

    // register Orleankka extension
    cb.ConfigureOrleankka() |> ignore

    let t = task {

        let host = sb.Build()
        do! host.StartAsync()    // start silo

        let client = cb.Build();
        do! client.Connect()     // connect client

        // get actor system
        let system = client.ActorSystem()

        // get a reference to the IGreeter actor
        // the actor will be automatically activated by Orleans on first use
        let actor = ActorSystem.typedActorOf<IGreeter, GreeterMessage>(system, "good-citizen")
      
        // use (<!) custom operator to send message to actor
        // this when you don't care about result (Task<unit>)
        // to ask for result use (<?)
        do! actor <! Hi
        do! actor <! Greet "Yevhen"
        do! actor <! Greet "World"      
    }
    t.Wait()
    
    Console.ReadKey() |> ignore 
    0
```

That is it. See more examples in a Samples directory.
