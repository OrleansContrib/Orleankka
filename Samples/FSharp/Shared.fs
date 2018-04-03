namespace Orleankka.FSharp

open Microsoft.Extensions.DependencyInjection

open System
open System.Linq
open System.Net
open System.Reflection
open FSharp.Control.Tasks
open Orleankka.Client
open Orleans
open Orleans.ApplicationParts
open Orleans.Hosting
open Orleans.Configuration
open Orleans.Runtime
open Orleans.Storage

[<AutoOpen>]
module Shared =

    let DemoClusterId = "localhost-demo"
    let LocalhostSiloPort = 11111
    let LocalhostGatewayPort = 30000
    let LocalhostSiloAddress = IPAddress.Loopback

    type ISiloHostBuilder with
  
      member sb.ConfigureDemoClustering() =    
        sb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId) |> ignore
        sb.UseDevelopmentClustering(fun (options:DevelopmentClusterMembershipOptions) -> options.PrimarySiloEndpoint <- IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort)) |> ignore
        sb.ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort) |> ignore
    
      member sb.AddAssembly(assembly:Assembly) =
        sb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(assembly).WithCodeGeneration() |> ignore) |> ignore

      member sb.Start() =
        sb.ConfigureDemoClustering()
        
        sb.AddMemoryGrainStorageAsDefault() |> ignore
        sb.AddMemoryGrainStorage("PubSubStore") |> ignore
        sb.AddSimpleMessageStreamProvider("sms") |> ignore
        sb.UseInMemoryReminderService() |> ignore

        sb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(typeof<MemoryGrainStorage>.Assembly) |> ignore) |> ignore
        
        task {
            let host = sb.Build()
            do! host.StartAsync()
            return host
        }

    type IClientBuilder with
  
      member cb.ConfigureDemoClustering() =
        cb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId) |> ignore
        cb.UseStaticClustering(fun (options:StaticGatewayListProviderOptions) -> options.Gateways.Add(IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri())) |> ignore
        
      member cb.AddAssembly(assembly:Assembly) =
        cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(assembly).WithCodeGeneration() |> ignore) |> ignore

        
    type ISiloHost with

      member host.Connect() =
        let cb = new ClientBuilder()
        cb.ConfigureDemoClustering()
        cb.AddSimpleMessageStreamProvider("sms") |> ignore
        cb.UseOrleankka() |> ignore

        cb.ConfigureApplicationParts(
          fun x -> let apm = host.Services.GetRequiredService<IApplicationPartManager>()
                   apm.ApplicationParts.OfType<AssemblyPart>()|> Seq.iter(fun part -> x.AddApplicationPart(part.Assembly).WithCodeGeneration() |> ignore)) |> ignore

        task {
            let client = cb.Build();
            do! client.Connect()
            return client
        }
