module Demo

open Orleankka
open Orleankka.Cluster
open Orleans.Runtime.Configuration
open Orleans.Providers.Streams.SimpleMessageStream

open Messages
open Actors

[<EntryPoint>]
let main argv = 
   
   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let system = 
    ActorSystem
     .Configure()
     .Cluster()
     .From(ClusterConfiguration.LocalhostPrimarySilo())
     .StreamProvider<SimpleMessageStreamProvider>("rooms")
     .Assemblies([|typeof<ChatUser>.Assembly;typeof<ChatRoomMessage>.Assembly|])
     .Done() 
   
   system.Start().Wait()
                   
   printfn "Finished booting cluster...\n"
   System.Console.ReadLine() |> ignore
      
   0