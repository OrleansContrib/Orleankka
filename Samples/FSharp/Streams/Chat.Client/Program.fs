module Demo

open System
open System.Net

open FSharp.Control.Tasks
open Orleankka
open Orleankka.Client
open Orleankka.FSharp
open Orleans
open Orleans.Hosting
open Orleans.Configuration
open Orleans.Runtime

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
   let roomStream = ActorSystem.streamOf(system, "sms", roomName)
   
   let chatClient = { UserName = userName; User = userActor;
                      RoomName = roomName; Room = roomStream;
                      Subscription = None }

   printfn "Joining the room '%s'..." roomName
   let! joinedClient = join chatClient

   printfn "Joined the room '%s'..." roomName
   return! handleUserInput joinedClient
}

let DemoClusterId = "localhost-demo"
let LocalhostGatewayPort = 30000
let LocalhostSiloAddress = IPAddress.Loopback

[<EntryPoint>]
let main argv = 

    printfn "Please wait until Chat Server has completed boot and then press enter. \n"
    Console.ReadLine() |> ignore

    let cb = new ClientBuilder()
    cb.Configure<ClusterOptions>(fun (options:ClusterOptions) -> options.ClusterId <- DemoClusterId) |> ignore
    cb.UseStaticClustering(fun (options:StaticGatewayListProviderOptions) -> options.Gateways.Add(IPEndPoint(LocalhostSiloAddress, LocalhostGatewayPort).ToGatewayUri())) |> ignore
    cb.AddSimpleMessageStreamProvider("sms") |> ignore
    cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(typeof<IChatUser>.Assembly).WithCodeGeneration() |> ignore) |> ignore
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

