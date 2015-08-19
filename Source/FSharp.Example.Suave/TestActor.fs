module Actors

open Orleankka.FSharp 

type HelloMessage =
   | Hi of string
   | WhatIsYourName
   | GiveMeMoney of currency:string * amount:double

type TestActor() =
   inherit Actor<HelloMessage>()
   
   override this.Receive message reply = task {
      match message with
      | Hi name        -> reply("hello " + name + "! I am a TestActor.")
      | WhatIsYourName -> reply("My name is TestActor")
      | GiveMeMoney (c,a) -> reply(sprintf "You want %f %s ??? Fuck you!!!" a c)
   }