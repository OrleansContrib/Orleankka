namespace Orleankka.FSharp.Configuration

[<RequireQualifiedAccess>]
module ClientConfig =   
   open System.Reflection
   open Orleans.Runtime.Configuration
   open Orleankka   
   open Orleankka.Client

   let inline create () = ClientConfiguration()

   let inline load input = ClientConfiguration().Load(input)

   let inline loadFromFile fileName = ClientConfiguration.LoadFromFile(fileName)

   let inline loadFromResource (assembly:Assembly) (fullResourcePath) =
      ClientConfiguration().LoadFromEmbeddedResource(assembly, fullResourcePath)

   let inline standardLoad () = ClientConfiguration.StandardLoad()

   let inline localhostSilo gatewayPort = ClientConfiguration.LocalhostSilo(gatewayPort)


[<RequireQualifiedAccess>]
module ClusterConfig =    
   open System.Reflection
   open Orleans.Runtime.Configuration
   open Orleankka
   open Orleankka.Cluster
   
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
   open Orleans.Runtime.Configuration
   open Orleankka
   open Orleankka.Playground
   open Orleankka.Client
   open Orleankka.Cluster

   let inline createClient config assemblies = 
      ActorSystem.Configure().Client().From(config).Register(assemblies).Done()

   let inline createCluster config assemblies =
      ActorSystem.Configure().Cluster().From(config).Register(assemblies).Done()

   let inline createPlayground assemblies =
      ActorSystem.Configure().Playground().Register(assemblies).Done()