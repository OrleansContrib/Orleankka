module Encryptor

open Orleankka
open Orleankka.FSharp
open System
open System.Text

type EncryptMessage =
   | Encrypt of messages:string seq

[<Worker>]
type EncryptWorker() =
   inherit Actor<string>()

   override this.Receive msg reply = task {
      msg 
      |> Encoding.Unicode.GetBytes
      |> Convert.ToBase64String
      |> reply      
   }

type Encryptor() as this =
   inherit Actor<EncryptMessage>()

   let worker = this.System.ActorOf<EncryptWorker>("encrypt_worker")

   override this.Receive msg reply = task {
      match msg with
      | Encrypt messages -> 
         messages
         |> Seq.map(worker.Ask)
         |> Task.whenAll(fun results -> results)
         |> reply     
   }