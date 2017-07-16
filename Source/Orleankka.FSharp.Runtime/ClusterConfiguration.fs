namespace Orleankka.FSharp.Runtime

open System.Reflection
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.Playground
open Orleankka.Cluster

module ClusterConfig =       
   
   let inline create () = ClusterConfiguration()

   let inline load input = ClusterConfiguration().Load(input)

   let inline loadFromFile fileName = ClusterConfiguration().LoadFromFile(fileName)

   let inline loadFromResource (assembly:Assembly) (fullResourcePath) =
      ClusterConfiguration().LoadFromEmbeddedResource(assembly, fullResourcePath)      

   let inline standardLoad () = ClusterConfiguration().StandardLoad()
      
   let inline localhostSilo siloPort gatewayPort =
      ClusterConfiguration.LocalhostPrimarySilo(siloPort, gatewayPort)
   
   let inline registerStreamProvider<'a when 'a :> Orleans.Streams.IStreamProvider > streamName props (config:ClusterConfiguration) =
      config.Globals.RegisterStreamProvider<'a>(streamName, props|> Map.toSeq |> dict)
      config


module ActorSystem =       
   let inline createCluster config assemblies =
      ActorSystem
        .Configure()
        .Cluster()
        .From(config)
        .Assemblies(assemblies : Assembly[])
   
   let inline interceptor<'a when 'a :> Orleankka.Cluster.IInterceptor> (cluster:ClusterConfigurator)=
      cluster.Interceptor<'a>()

   let inline bootstrapper<'a when 'a :> Orleankka.Cluster.IBootstrapper> props  (cluster:ClusterConfigurator)= 
      cluster.Bootstrapper<'a>(props |> Map.toSeq |> dict)

   let inline complete (cluster: ClusterConfigurator)= 
      cluster.Done()

   let inline createPlayground assemblies =
      ActorSystem.Configure().Playground().Assemblies(assemblies : Assembly[]).Done()
   