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

   override this.Receive message reply = task {
      match message with
      | Greet who -> sprintf "Receive Hello %s" who |> reply
      | Hi        -> sprintf "Receive Hi"           |> reply
   }

type TestActorUntyped() = 
   inherit Actor<obj>()   

   let handleMessage = function
      | Greet who -> sprintf "Receive Hello %s" who
      | Hi -> sprintf "Receive Hi"

   let handleInt value = sprintf "Got int %i" value

   let handleStr value = sprintf "Got string %s" value

   override this.Receive message reply = task {
      match message with
      | :? Message as m -> m |> handleMessage |> reply
      | :? int as i     -> i |> handleInt     |> reply
      | :? string as s  -> s |> handleStr     |> reply
      | _               -> failwith "Received unexpected message type"
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
      let actor = this.system.ActorOf<TestActor>("test")
      match Task.run(fun _ -> task {return! actor <? "request msg"}) with
      | Choice1Of2 result -> Assert.Fail("actor was able to handle unspecified message type.")
      | Choice2Of2 ex     -> Assert.IsInstanceOf(typeof<Exception>, ex.InnerException)