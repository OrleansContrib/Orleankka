module Orleankka.FSharp.RequestResponse

open NUnit.Framework
open System
open Orleankka
open Orleankka.FSharp.Tests.Infrastructure

type Message = 
   | Greet of string
   | Hi

type TestActor() = 
   inherit Actor<Message>()

   override this.Receive(message, reply) = task {
      match message with
      | Greet who -> sprintf "Receive Hello %s" who |> reply
      | Hi -> sprintf "Receive Hi" |> reply
   }

   override this.ReceiveAny(message, reply) = task {
      match message with
      | :? int as i -> sprintf "ReceiveAny int %i" i |> reply
      | _           -> do! this.ReceiveAnyBase(message, reply)
   }

   member this.ReceiveAnyBase(message, reply) = base.ReceiveAny(message, reply)

[<TestFixture>]
[<RequiresSilo>]
type Tests() =   
   [<DefaultValue>] val mutable system: IActorSystem

   [<SetUp>]
   member this.SetUp() = 
      this.system <- TestActorSystem.instance

   [<Test>]
   member this.``ReceiveAny should be overriden to receive all and every message sent to an actor.``() = 
      let actor = this.system.ActorOf<TestActor>("test");

      Assert.AreEqual("ReceiveAny int 1", actor.Ask(1).Result)
      Assert.Throws<AggregateException>(fun ()-> actor.Tell("hello").Wait()) |> ignore

   [<Test>]
   member this.``Receive should be invoked in case of receiving actor's message type.``() =
      let actor = this.system.ActorOf<TestActor>("test");

      Assert.AreEqual("Receive Hi", actor.Ask(Hi).Result)