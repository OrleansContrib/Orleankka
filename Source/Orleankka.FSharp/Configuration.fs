namespace Orleankka.FSharp.Configuration
open System
open System.Reflection
open System.Threading.Tasks
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.FSharp
open Orleankka.Client

module ClientConfig =      

   let inline create () = ClientConfiguration()

   let inline load input = ClientConfiguration().Load(input)

   let inline loadFromFile fileName = ClientConfiguration.LoadFromFile(fileName)

   let inline loadFromResource (assembly:Assembly) (fullResourcePath) =
      ClientConfiguration().LoadFromEmbeddedResource(assembly, fullResourcePath)

   let inline standardLoad () = ClientConfiguration.StandardLoad()

   let inline localhostSilo gatewayPort = ClientConfiguration.LocalhostSilo(gatewayPort)

   let inline registerStreamProvider<'a when 'a :> Orleans.Streams.IStreamProvider > streamName props (config:ClientConfiguration) =
      config.RegisterStreamProvider<'a>(streamName, props|> Map.toSeq |> dict)
      config


[<RequireQualifiedAccess>]
module ActorSystem =       

   let createConfiguredClient config actors assemblies = 
      ActorSystem.Configure().Client().From(config).Assemblies(assemblies : Assembly[]).ActorTypes(actors: string[]).Done()
 
   let inline start (system:^TSys) = 
      (^TSys: (member Start: wait:bool -> Task) (system, false))
      |> Task.WaitAny
      |> ignore
      system

   let inline stop (system:^TSys) = 
      (^TSys: (member Stop: force:bool -> Task) (system, false))
      |> Task.WaitAny
      |> ignore
      
   let inline forceStop (system:^TSys) = 
      (^TSys: (member Stop: force:bool -> Task) (system, true))      
      |> Task.WaitAny
      |> ignore

   /// THIS NEED TO FIXED PROPERLY. Await should be done by consumer 
   let inline connect (system:^TSys) =
      (^TSys: (member Connect: retries:int -> retryTimeout:Nullable<TimeSpan> -> Task) (system, 0, new Nullable<TimeSpan>()))
      |> Task.WaitAny
      |> ignore
      system
      
   let inline actorOf<'TActor when 'TActor :> IActor> (system:IActorSystem, actorId) =
      let actorPath = typeof<'TActor>.ToActorPath(actorId) 
      system.ActorOf(actorPath) |> FSharp.ActorRef<obj>

   let inline actorOfPath(system:IActorSystem) path =
      system.ActorOf(path) |> FSharp.ActorRef<obj>

   let inline typedActorOf<'TActor when 'TActor :> IActor> (system:IActorSystem, actorId) =
      let actorPath = typeof<'TActor>.ToActorPath(actorId) 
      system.ActorOf(actorPath) |> FSharp.ActorRef<'TActor>

   let inline streamOf (system:IActorSystem) provider streamId = 
      system.StreamOf(provider, streamId) |> FSharp.StreamRef<'TMsg>