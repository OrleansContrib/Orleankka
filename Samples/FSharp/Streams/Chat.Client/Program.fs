module Demo

open System

open FSharp.Control.Tasks
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

   let userActor = ActorSystem.typedActorOf<IChatUser, ChatUserMessage>(system, userName)
   let roomStream = ActorSystem.streamOf(system, "rooms", roomName)
   
   let chatClient = { UserName = userName; User = userActor;
                      RoomName = roomName; Room = roomStream;
                      Subscription = None }

   printfn "Joining the room '%s'..." roomName
   let! joinedClient = join chatClient

   printfn "Joined the room '%s'..." roomName
   return! handleUserInput joinedClient
}

open Orleankka.Client
open Orleans
open Orleans.Hosting
open Orleans.Runtime.Configuration

[<EntryPoint>]
let main argv = 

    printfn "Please wait until Chat Server has completed boot and then press enter. \n"
    Console.ReadLine() |> ignore

    let cc = ClientConfiguration.LocalhostSilo()
    cc.AddSimpleMessageStreamProvider("rooms")

    let cb = new ClientBuilder()
    cb.UseConfiguration(cc) |> ignore
    cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(typedefof<IChatUser>.Assembly).WithCodeGeneration() |> ignore) |> ignore
    cb.ConfigureOrleankka() |> ignore

    let client = cb.Build()
    client.Connect().Wait()
   
    printfn "Enter your user name..."
    let userName = Console.ReadLine();

    printfn "Enter a room which you'd like to join..."
    let roomName = Console.ReadLine();

    let system = client.ActorSystem()
    let t = startChatClient system userName roomName 
    t.Wait()

    Console.ReadLine() |> ignore  
    0 // return an integer exit code

