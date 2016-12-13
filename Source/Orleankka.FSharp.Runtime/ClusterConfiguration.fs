namespace Orleankka.FSharp.Runtime

open System.Reflection
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.FSharp
open Orleankka.Playground
open Orleankka.Cluster

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
   let inline createCluster config assemblies =
      ActorSystem.Configure().Cluster().From(config).Assemblies(assemblies : Assembly[]).Done()

   let inline createPlayground assemblies =
      ActorSystem.Configure().Playground().Assemblies(assemblies : Assembly[]).Done()
        