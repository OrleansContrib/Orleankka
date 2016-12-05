open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration
open Messages

[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let assembly = Assembly.GetExecutingAssembly()

   let config = ClusterConfig.loadFromResource assembly "Server.xml"   

   use system = [|assembly;typeof<ServerMessage>.Assembly|]
                |> ActorSystem.createCluster config
                |> ActorSystem.start   
   
   printfn "Finished booting cluster...\n"
   System.Console.ReadLine() |> ignore
      
   0