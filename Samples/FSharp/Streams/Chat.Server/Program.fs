module Demo

open Microsoft.Extensions.DependencyInjection
open System.Net
open System.Reflection
open Orleankka.Cluster
open Orleans
open Orleans.Hosting
open Orleans.ApplicationParts
open Orleans.Configuration
open Orleans.Storage

open Messages

let DemoClusterId = "localhost-demo"
let LocalhostSiloPort = 11111
let LocalhostGatewayPort = 30000
let LocalhostSiloAddress = IPAddress.Loopback

[<EntryPoint>]
let main argv = 
   
    printfn "Running demo. Booting cluster might take some time ...\n"
   
    let configureAssemblies (apm:IApplicationPartManager) =
        apm.AddApplicationPart(typeof<IChatUser>.Assembly).WithCodeGeneration() |> ignore
        apm.AddApplicationPart(typeof<MemoryGrainStorage>.Assembly).WithCodeGeneration() |> ignore
        apm.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore

    let sb = SiloHostBuilder()
    sb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId) |> ignore
    sb.UseDevelopmentClustering(fun (options:DevelopmentClusterMembershipOptions) -> options.PrimarySiloEndpoint <- IPEndPoint(LocalhostSiloAddress, LocalhostSiloPort)) |> ignore
    sb.ConfigureEndpoints(LocalhostSiloAddress, LocalhostSiloPort, LocalhostGatewayPort) |> ignore

    sb.AddMemoryGrainStorageAsDefault() |> ignore
    sb.AddMemoryGrainStorage("PubSubStore") |> ignore
    sb.AddSimpleMessageStreamProvider("sms") |> ignore
    sb.UseInMemoryReminderService() |> ignore

    sb.ConfigureApplicationParts(fun x -> configureAssemblies x) |> ignore
    sb.ConfigureOrleankka() |> ignore

    use host = sb.Build()
    host.StartAsync().Wait()
                   
    printfn "Finished booting cluster...\n"
    System.Console.ReadLine() |> ignore
      
    0