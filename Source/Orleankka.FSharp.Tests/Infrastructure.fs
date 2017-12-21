module Orleankka.FSharp.Tests.Infrastructure

open System
open NUnit.Framework
open Orleankka
open Orleankka.Playground
open Orleankka.Embedded
open NUnit.Framework.Interfaces

module TestActorSystem =    
   let mutable instance:EmbeddedActorSystem = null
   
[<AttributeUsage(AttributeTargets.Class)>]
type RequiresSiloAttribute() =
   inherit TestActionAttribute()

   override this.BeforeTest(details:ITest) =
      if details.IsSuite then this.StartNew()

   member private this.StartNew() =
      if isNull(TestActorSystem.instance) then
         let system = ActorSystem.Configure()
                                 .Playground()
                                 .UseInMemoryPubSubStore()
                                 .Assemblies(this.GetType().Assembly)
                                 .Done()
      
         TestActorSystem.instance <- system
         TestActorSystem.instance.Start().Wait()

type TeardownSiloAttribute() =
   inherit TestActionAttribute()

   override this.AfterTest(details:ITest) =
      if details.IsSuite then
        if (TestActorSystem.instance <> null) then
            TestActorSystem.instance.Stop().Wait()
            TestActorSystem.instance.Dispose()
            TestActorSystem.instance <- null

[<assembly: TeardownSilo()>]
()