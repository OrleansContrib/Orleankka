module Orleankka.FSharp.RequestResponse

open NUnit.Framework
open System
open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Tests.Infrastructure

type Message = 
   | Greet of string
   | Hi

type ITestActor = 
    inherit IActorGrain

type TestActor() = 
   inherit ActorGrain<Message>()
   interface ITestActor

   override this.Receive message = task {
      match message with
      | Greet who -> return response(sprintf "Receive Hello %s" who)
      | Hi        -> return response(sprintf "Receive Hi")
   }

[<TestFixture>]
[<RequiresSilo>]
type Tests() =   
   [<DefaultValue>] val mutable system: IActorSystem

   [<SetUp>]
   member this.SetUp() = 
      this.system <- TestActorSystem.instance
    
   [<Test>]
   member this.``Actor<T> should throws an exception when input message type is different then T type.``() = 
      let actor = ActorSystem.actorOf<ITestActor>(this.system, "test")
      match Task.run(fun _ -> task {return! actor <? "request msg"}) with
      | Choice1Of2 result -> Assert.Fail("actor was able to handle unspecified message type.")
      | Choice2Of2 ex     -> Assert.IsInstanceOf(typeof<Exception>, ex.InnerException)