﻿open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration
open Orleankka.FSharp.Runtime

open Messages

[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let assembly = Assembly.GetExecutingAssembly()

   let config = ClusterConfig.loadFromResource assembly "Server.xml"   

   let system = [|assembly;typeof<ServerMessage>.Assembly|]
                |> ActorSystem.createCluster config
                |> ActorSystem.complete
                |> ActorSystem.start   
   
   printfn "Finished booting cluster...\n"
   System.Console.ReadLine() |> ignore
      
   0