module Orleankka.FSharp.Tests.Infrastructure

open System
open NUnit.Framework
open Orleankka
open Orleankka.Playground

module TestActorSystem =    
   let mutable instance:IActorSystem = null
   
[<AttributeUsage(AttributeTargets.Class)>]
type RequiresSiloAttribute() =
   inherit TestActionAttribute()

   override this.BeforeTest(details:TestDetails) =
      if details.IsSuite = true then this.StartNew()

   member private this.StartNew() =
      if isNull(TestActorSystem.instance) then
         let system = ActorSystem.Configure()
                                 .Playground()
                                 .UseInMemoryPubSubStore()
                                 .Register(this.GetType().Assembly)
                                 .Done()
      
         TestActorSystem.instance <- system