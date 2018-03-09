module Demo

open System
open System.Reflection
open Orleankka.Cluster
open Orleans
open Orleans.Hosting
open Orleans.Runtime.Configuration
open Orleans.ApplicationParts

open Messages

[<EntryPoint>]
let main argv = 
   
    printfn "Running demo. Booting cluster might take some time ...\n"
   
    let sc = ClusterConfiguration.LocalhostPrimarySilo()
                  
    sc.AddMemoryStorageProvider()
    sc.AddMemoryStorageProvider("PubSubStore")
    sc.AddSimpleMessageStreamProvider("rooms")

    let configureAssemblies (apm:IApplicationPartManager) =
        apm.AddApplicationPart(typedefof<IChatUser>.Assembly).WithCodeGeneration() |> ignore
        apm.AddApplicationPart(Assembly.GetExecutingAssembly()).WithCodeGeneration() |> ignore

    let sb = new SiloHostBuilder()
    sb.UseConfiguration(sc) |> ignore
    sb.ConfigureApplicationParts(fun x -> configureAssemblies x) |> ignore
    sb.ConfigureOrleankka() |> ignore

    use host = sb.Build()
    host.StartAsync().Wait()
                   
    printfn "Finished booting cluster...\n"
    System.Console.ReadLine() |> ignore
      
    0