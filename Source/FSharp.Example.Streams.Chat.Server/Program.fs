open System.Reflection

open Orleans.Providers.Streams.SimpleMessageStream
open Orleankka.FSharp.Runtime

open Messages
open Actors


[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let assembly = Assembly.GetExecutingAssembly()

   let config = ClusterConfig.loadFromResource assembly "Server.xml"   
                |> ClusterConfig.registerStreamProvider<SimpleMessageStreamProvider> "rooms" Map.empty

   use system = [|typeof<ChatUser>.Assembly;typeof<ChatRoomMessage>.Assembly|]
                |> ActorSystem.createCluster config 
                |> ActorSystem.complete

   system.Start() 
                   
   printfn "Finished booting cluster...\n"
   System.Console.ReadLine() |> ignore
      
   0