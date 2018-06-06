module Demo

open System

open Orleankka
open Orleankka.FSharp

open Messages
open Client.ChatClient

let rec handleUserInput client = task {
   
   let message = Console.ReadLine();

   match message with
   | "::leave" -> do! leave client
   
   | _         -> do! say client message
                  return! handleUserInput client
}

let startChatClient (system:IActorSystem) userName roomName = task {

   let userPath = ActorPath.From("ChatUser", userName)
   let userActor = ActorSystem.actorOfPath system userPath
   let roomStream = ActorSystem.streamOf system "rooms" roomName
   
   let chatClient = { UserName = userName; User = userActor;
                      RoomName = roomName; Room = roomStream;
                      Subscription = None }

   printfn "Joining the room '%s'..." roomName

   let! joinedClient = join chatClient

   printfn "Joined the room '%s'..." roomName
   
   return! handleUserInput joinedClient
}

open Orleankka.Client
open Orleans.Providers.Streams.SimpleMessageStream
open Orleans.Runtime.Configuration

[<EntryPoint>]
let main argv = 

   printfn "Please wait until Chat Server has completed boot and then press enter. \n"
   Console.ReadLine() |> ignore

   let system = 
    ActorSystem
     .Configure()
     .Client()
     .From(ClientConfiguration.LocalhostSilo())
     .UseSimpleMessageStreamProvider("rooms")
     .Assemblies([|typeof<ChatRoomMessage>.Assembly|])
     .ActorTypes([|"ChatUser"|])
     .Done() 
   
   system.Connect().Wait()

   printfn "Enter your user name..."
   let userName = Console.ReadLine();

   printfn "Enter a room which you'd like to join..."
   let roomName = Console.ReadLine();

   let t = startChatClient system userName roomName 
   t.Wait()

   Console.ReadLine() |> ignore  
   0 // return an integer exit code

