open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration
open Orleankka.FSharp.Runtime

open Messages
open Actors


[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let assembly = Assembly.GetExecutingAssembly()

   let config = ClusterConfig.loadFromResource assembly "Server.xml"   
                |> ClusterConfig.registerStreamProvider<Orleans.Providers.Streams.SimpleMessageStream.SimpleMessageStreamProvider> "sms2" Map.empty

   use system = [|typeof<ChatUser>.Assembly;typeof<ChatRoomMessage>.Assembly|]
                |> ActorSystem.createCluster config 
                |> ActorSystem.start   
   
   printfn "Finished booting cluster...\n"
   System.Console.ReadLine() |> ignore
      
   0