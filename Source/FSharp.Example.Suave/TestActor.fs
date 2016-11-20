module Actors

open Orleankka.CSharp
open Orleankka.FSharp 

type HelloMessage =
   | Hi of string
   | WhatIsYourName
   | GiveMeMoney of currency:string * amount:double

[<ActorType("TestActor")>]
type TestActor() =
   inherit Actor<HelloMessage>()
   
   override this.Receive message = task {
      match message with
      | Hi name           -> return response("hello " + name + "! I am a TestActor.")
      | WhatIsYourName    -> return response("My name is TestActor")
      | GiveMeMoney (c,a) -> return response(sprintf "You want %f %s ??? Fuck you!!!" a c)
   }