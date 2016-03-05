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

   override this.ReceiveUntyped(message, reply) = task {
      match message with
      | :? int as i -> sprintf "ReceiveUntyped int %i" i |> reply
      | _           -> let typeName = message.GetType().Name
                       sprintf "Got type %s" typeName |> reply
   }


[<TestFixture>]
[<RequiresSilo>]
type Tests() =   
   [<DefaultValue>] val mutable system: IActorSystem

   [<SetUp>]
   member this.SetUp() = 
      this.system <- TestActorSystem.instance

   [<Test>]
   member this.``ReceiveUntyped should be invoked in case of receiving not actor's message type.``() = 
      let actor = this.system.ActorOf<TestActor>("test");

      let response1 = actor.Ask(1).Result
      let response2 = actor.Ask("hello").Result
      let response3 = actor.Ask(true).Result

      Assert.AreEqual("ReceiveUntyped int 1", response1)
      Assert.AreEqual("Got type String", response2)
      Assert.AreEqual("Got type Boolean", response3)

   [<Test>]
   member this.``Receive should be invoked in case of receiving actor's message type.``() =
      let actor = this.system.ActorOf<TestActor>("test");

      let response = actor.Ask(Hi).Result

      Assert.AreEqual("Receive Hi", response)