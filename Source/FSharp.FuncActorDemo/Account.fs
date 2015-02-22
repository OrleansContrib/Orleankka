module Account

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.FuncActor

type AccountMessage = 
   | Deposit of int
   | Withdraw of int
   | Balance

let AccountActor = actor {
   name "Account"
   init (fun balance -> 0)
   receive (fun balance message context -> task {
      match message with
      | Deposit amount   -> return balance + amount
      | Withdraw amount  -> return balance - amount
      | Balance          -> context.Response <- balance
                            return balance
   })
}

