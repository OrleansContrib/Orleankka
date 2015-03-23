
open Orleans.Runtime.Configuration
open Orleankka.FSharp.System
open Orleankka.Cluster
open System.Reflection

[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let assembly = Assembly.GetExecutingAssembly()

   let config = ClusterConfiguration().LoadFromEmbeddedResource(assembly, "Server.xml")

   use system = config 
                |> initClusterActorSystem
                |> register [|assembly|]
                |> start

   printfn "Finished booting cluster...\n"

   System.Console.ReadLine() |> ignore
      
   0
