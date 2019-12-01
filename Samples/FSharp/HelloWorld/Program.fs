module Demo

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

type GreeterMessage = 
   | Greet of string
   | Hi

type IGreeter = 
   inherit IGrainWithStringKey
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

    let DemoClusterId = "localhost-demo"
    let DemoServiceId = "localhost-demo-service"
    let LocalhostSiloPort = 11111
    let LocalhostGatewayPort = 30000
    let LocalhostSiloAddress = IPAddress.Loopback

    printfn "Running demo. Booting cluster might take some time ...\n"

    let sb = new SiloHostBuilder()
    sb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId; options.ServiceId <- DemoServiceId) |> ignore
    sb.UseDevelopmentClustering(fun (options:DevelopmentClusterMembershipOptions) -> options.PrimarySiloEndpoint <- IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort)) |> ignore
    sb.ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort) |> ignore
    sb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore) |> ignore
    sb.UseOrleankka() |> ignore
  
    let cb = new ClientBuilder()
    cb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId; options.ServiceId <- DemoServiceId) |> ignore
    cb.UseStaticClustering(fun (options:StaticGatewayListProviderOptions) -> options.Gateways.Add(IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri())) |> ignore
    cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore) |> ignore
    cb.UseOrleankka() |> ignore

    let t = task {

        let host = sb.Build()
        do! host.StartAsync()

        let client = cb.Build();
        do! client.Connect()

        let system = client.ActorSystem()
        let actor = ActorSystem.typedActorOf<IGreeter, GreeterMessage>(system, "good-citizen")
      
        do! actor <! Hi
        do! actor <! Greet "Yevhen"
        do! actor <! Greet "World"      
    }
    t.Wait()
    
    Console.ReadKey() |> ignore 
    0