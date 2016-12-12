module Orleankka.FSharp.Tests.Infrastructure

open System
open NUnit.Framework
open Orleankka
open Orleankka.Playground
open Orleankka.Embedded

module TestActorSystem =    
   let mutable instance:EmbeddedActorSystem = null
   
[<AttributeUsage(AttributeTargets.Class)>]
type RequiresSiloAttribute() =
   inherit TestActionAttribute()

   override this.BeforeTest(details:TestDetails) =
      if details.IsSuite then this.StartNew()

   member private this.StartNew() =
      if isNull(TestActorSystem.instance) then
         let system = ActorSystem.Configure()
                                 .Playground()
                                 .UseInMemoryPubSubStore()
                                 .Register(this.GetType().Assembly)
                                 .Done()
      
         TestActorSystem.instance <- system
         TestActorSystem.instance.Start()

type TeardownSiloAttribute() =
   inherit TestActionAttribute()

   override this.AfterTest(details:TestDetails) =
      if details.IsSuite then
        if (TestActorSystem.instance <> null) then
            TestActorSystem.instance.Dispose()
            TestActorSystem.instance <- null

[<assembly: TeardownSilo()>]
()