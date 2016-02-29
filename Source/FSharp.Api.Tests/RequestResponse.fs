module Orleankka.FSharp.RequestResponse

open NUnit.Framework
open System.Threading
open System.Threading.Tasks
open Orleankka
open Orleankka.Testing

type Message = 
   | Greet of string
   | Hi

type TestActor() = 
   inherit Actor<Message>()

   override this.Receive message reply = task {
      match message with
      | Greet who -> sprintf "Hello %s" who |> reply
      | Hi -> sprintf "Receive Hi" |> reply
   }

   override this.UntypedReceive message reply = task {
      match message with
      | :? int as i -> sprintf "UntypedReceive got int %i" i |> reply
      | _           -> let typeName = message.GetType().Name
                       sprintf "Got type %s" typeName |> reply
   }


[<TestFixture>]
[<RequiresSilo>]
type Tests() =   
   [<DefaultValue>] val mutable system: IActorSystem

   [<SetUp>]
   member this.SetUp() = 
      this.system <- TestActorSystem.Instance

   [<Test>]
   member this.``UntypedReceive should be invoked in case of receiving not actor's message type.``() = 
      let actor = this.system.ActorOf<TestActor>("test");

      let response = actor.Ask(1).Result

      Assert.AreEqual("UntypedReceive got int 1", response)

   [<Test>]
   member this.``Receive should be invoked in case of receiving actor's message type.``() =
      let actor = this.system.ActorOf<TestActor>("test");

      let response = actor.Ask(Hi).Result

      Assert.AreEqual("Receive Hi", response)