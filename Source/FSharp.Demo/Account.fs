module Account

open Orleankka
open Orleankka.FSharp

type AccountMessage = 
   | Deposit of int
   | Withdraw of int
   | Balance 
   
type Account() = 
   inherit BaseActor<AccountMessage>()

   let mutable balance = 0   
   
   override this.Receive(message) = task {
      match message with
      | Deposit amount   -> balance <- balance + amount
                            return Empty

      | Withdraw amount  -> balance <- balance - amount
                            return Empty

      | Balance          -> return Result(balance)
   }
