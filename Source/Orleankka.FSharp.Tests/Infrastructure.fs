module Orleankka.FSharp.Tests.Infrastructure

open System
open NUnit.Framework
open NUnit.Framework.Interfaces
open Orleankka
open Orleankka.Client
open Orleankka.Cluster
open Orleans
open Orleans.Hosting
open Orleans.Runtime.Configuration

module TestActorSystem =    
   let mutable host:ISiloHost = null
   let mutable client:IClusterClient = null
   let mutable instance:IActorSystem = null
   
[<AttributeUsage(AttributeTargets.Class)>]
type RequiresSiloAttribute() =
   inherit TestActionAttribute()

   override this.BeforeTest(details:ITest) =
      if details.IsSuite then this.StartNew()

   member private this.StartNew() =
      if isNull(TestActorSystem.instance) then

        let sc = ClusterConfiguration.LocalhostPrimarySilo()
                  
        sc.AddMemoryStorageProvider()
        sc.AddMemoryStorageProvider("PubSubStore")
        sc.AddSimpleMessageStreamProvider("sms")

        let sb = new SiloHostBuilder()
        sb.UseConfiguration(sc) |> ignore
        sb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(this.GetType().Assembly).WithCodeGeneration() |> ignore) |> ignore
        sb.ConfigureOrleankka() |> ignore

        let host = sb.Build()
        host.StartAsync().Wait()

        let cc = ClientConfiguration.LocalhostSilo()
        cc.AddSimpleMessageStreamProvider("sms")

        let cb = new ClientBuilder()
        cb.UseConfiguration(cc) |> ignore
        cb.ConfigureApplicationParts(fun x -> x.AddApplicationPart(this.GetType().Assembly).WithCodeGeneration() |> ignore) |> ignore
        cb.ConfigureOrleankka() |> ignore

        let client = cb.Build()
        client.Connect().Wait()

        TestActorSystem.host <- host
        TestActorSystem.client <- client
        TestActorSystem.instance <- client.ActorSystem()

type TeardownSiloAttribute() =
   inherit TestActionAttribute()

   override this.AfterTest(details:ITest) =
      if details.IsSuite then
        if (TestActorSystem.instance <> null) then
            TestActorSystem.client.Close().Wait()
            TestActorSystem.client.Dispose()
            TestActorSystem.host.StopAsync().Wait()
            TestActorSystem.host.Dispose()
            
            TestActorSystem.client <- null;
            TestActorSystem.host <- null;
            TestActorSystem.instance <- null;

[<assembly: TeardownSilo()>]
()