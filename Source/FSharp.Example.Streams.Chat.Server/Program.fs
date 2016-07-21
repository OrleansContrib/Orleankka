open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration
open Actors


[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let assembly = Assembly.GetExecutingAssembly()

   let config = ClusterConfig.loadFromResource assembly "Server.xml"   

   use system = ActorSystem.createCluster config [|typeof<ChatUser>.Assembly|]
   system.Start()
   
   printfn "Finished booting cluster...\n"
   System.Console.ReadLine() |> ignore
      
   0