module Account

open System
open System.Reflection

open Orleans
open Orleans.Runtime.Configuration

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.System

type AccountMessage = 
   | Deposit of int
   | Withdraw of int
   | Balance 
   
type Account() as this = 
   inherit FunActor()

   let mutable balance = 0
   
   do 
      this.Receive(fun message -> task {         
         match message with
         | Deposit amount   -> balance <- balance + amount
         | Withdraw amount  -> balance <- balance - amount         
         | Balance          -> this.Reply(balance)
      })         
