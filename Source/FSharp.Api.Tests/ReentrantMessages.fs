module Orleankka.FSharp.ReentrantMessages

open NUnit.Framework
open System
open Orleankka
open Orleankka.FSharp.Tests.Infrastructure
open System.Runtime.Remoting.Messaging

type RegularMessage = {
   Data: string 
} 

type ReentrantMessage = {
   Data: string
}

type ReentrantMessage2() = class end

[<Reentrant(typeof<ReentrantMessage>)>]
[<Reentrant(typeof<ReentrantMessage2>)>]  
type TestActor() = 
   inherit Actor<obj>()

   let receivedReentrant msg = CallContext.LogicalGetData("LastMessageReceivedReentrant") = msg

   override this.Receive message reply = task {
      match message with
      | :? RegularMessage as m    -> m |> receivedReentrant |> reply
      
      | :? ReentrantMessage as m  -> m |> receivedReentrant |> reply

      | :? ReentrantMessage2 as m -> m |> receivedReentrant |> reply
      
      | _                         -> failwith "Received unexpected message type"
   }

[<TestFixture>]
[<RequiresSilo>]
type Tests() =   
   [<DefaultValue>] val mutable system: IActorSystem

   [<SetUp>]
   member this.SetUp() = 
      this.system <- TestActorSystem.instance

   [<Test>]
   member this.``Could be defined via attribute.``() = 
      let actor = this.system.ActorOf<TestActor>("test")
      
      match Task.run(fun _ -> task {return! actor <? { RegularMessage.Data = "test" }}) with
      | Choice1Of2 (result:bool) -> Assert.That(result, Is.False)
      | Choice2Of2 ex            -> Assert.Fail(ex.ToString())

      match Task.run(fun _ -> task {return! actor <? { ReentrantMessage.Data = "test" }}) with
      | Choice1Of2 (result:bool) -> Assert.That(result, Is.True)
      | Choice2Of2 ex            -> Assert.Fail(ex.ToString())

      match Task.run(fun _ -> task {return! actor <? ReentrantMessage2() }) with
      | Choice1Of2 (result:bool) -> Assert.That(result, Is.True)
      | Choice2Of2 ex            -> Assert.Fail(ex.ToString())