namespace Orleankka.FSharp.Configuration
open System.Reflection
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.CSharp
open Orleankka.FSharp
open Orleankka.Playground
open Orleankka.Client
open Orleankka.Cluster

[<RequireQualifiedAccess>]
module ClientConfig =      

   let inline create () = ClientConfiguration()

   let inline load input = ClientConfiguration().Load(input)

   let inline loadFromFile fileName = ClientConfiguration.LoadFromFile(fileName)

   let inline loadFromResource (assembly:Assembly) (fullResourcePath) =
      ClientConfiguration().LoadFromEmbeddedResource(assembly, fullResourcePath)

   let inline standardLoad () = ClientConfiguration.StandardLoad()

   let inline localhostSilo gatewayPort = ClientConfiguration.LocalhostSilo(gatewayPort)


[<RequireQualifiedAccess>]
module ClusterConfig =       
   
   let inline create () = ClusterConfiguration()

   let inline load input = ClusterConfiguration().Load(input)

   let inline loadFromFile fileName = ClusterConfiguration().LoadFromFile(fileName)

   let inline loadFromResource (assembly:Assembly) (fullResourcePath) =
      ClusterConfiguration().LoadFromEmbeddedResource(assembly, fullResourcePath)      

   let inline standardLoad () = ClusterConfiguration().StandardLoad()
      
   let inline localhostSilo siloPort gatewayPort =
      ClusterConfiguration.LocalhostPrimarySilo(siloPort, gatewayPort)
   

[<RequireQualifiedAccess>]
module ActorSystem =       

   let inline createClient config assemblies = 
      ActorSystem.Configure().Client().From(config).CSharp(fun x -> x.Register(assemblies) |> ignore).Done()

   let inline createCluster config assemblies =
      ActorSystem.Configure().Cluster().From(config).CSharp(fun x -> x.Register(assemblies) |> ignore).Done()

   let inline createPlayground assemblies =
      ActorSystem.Configure().Playground().CSharp(fun x -> x.Register(assemblies) |> ignore).Done()
         
   let inline start (system:^TSys) = 
      (^TSys: (member Start: wait:bool -> unit) (system, false))
      system

   let inline stop (system:^TSys) = 
      (^TSys: (member Stop: force:bool -> unit) (system, false))      

   let inline forceStop (system:^TSys) = 
      (^TSys: (member Stop: force:bool -> unit) (system, true))      

   let inline conect (system:^TSys) =
      (^TSys: (member Connect: retries:int -> unit) (system, 0))
      system   
      
   let inline disconnect (system:^TSys) =
      (^TSys: (member Disconnect: unit -> unit) (system))

   let inline actorOf<'TActor when 'TActor :> IActor> (system:IActorSystem, actorId) =
      let actorPath = typeof<'TActor>.ToActorPath(actorId) 
      system.ActorOf(actorPath) |> FSharp.ActorRef<obj>

   let inline typedActorOf<'TActor when 'TActor :> IActor> (system:IActorSystem, actorId) =
      let actorPath = typeof<'TActor>.ToActorPath(actorId) 
      system.ActorOf(actorPath) |> FSharp.ActorRef<'TActor>

   let inline streamOf (system:IActorSystem) provider streamId = 
      system.StreamOf(provider, streamId) |> FSharp.StreamRef<'TMsg>