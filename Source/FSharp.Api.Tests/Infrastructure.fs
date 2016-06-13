module Orleankka.FSharp.Tests.Infrastructure

open System
open NUnit.Framework
open Orleankka
open Orleankka.CSharp
open Orleankka.Playground

module TestActorSystem =    
   let mutable instance:IActorSystem = null
   
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
                                 .CSharp(fun x -> x.Register(this.GetType().Assembly) |> ignore)
                                 .Done()
      
         TestActorSystem.instance <- system

type TeardownSiloAttribute() =
   inherit TestActionAttribute()

   override this.AfterTest(details:TestDetails) =
      if details.IsSuite then
        if (TestActorSystem.instance <> null) then
            TestActorSystem.instance.Dispose()
            TestActorSystem.instance <- null

[<assembly: TeardownSilo()>]
()