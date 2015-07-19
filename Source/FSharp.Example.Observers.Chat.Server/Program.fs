
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.Cluster
open System.Reflection

[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let assembly = Assembly.GetExecutingAssembly()

   let config = ClusterConfiguration().LoadFromEmbeddedResource(assembly, "Server.xml")

   use system = ActorSystem.Configure()
                           .Cluster()
                           .From(config) 
                           .Register(assembly)
                           .Done()

   printfn "Finished booting cluster...\n"

   System.Console.ReadLine() |> ignore
      
   0
