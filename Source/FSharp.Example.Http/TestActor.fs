module Actors

open Orleankka.FSharp 

type HelloMessage =
   | Hi of string
   | WhatIsYourName

type TestActor() =
   inherit Actor<HelloMessage>()
   
   override this.Receive message reply = task {
      match message with
      | Hi name        -> reply("hello " + name + "! I am a TestActor.")
      | WhatIsYourName -> reply("My name is TestActor")
   }