module Account

open Orleankka.FSharp

type AccountMessage = 
   | Deposit of int
   | Withdraw of int
   | Balance 
   
type Account() = 
   inherit Actor<AccountMessage>()

   let mutable balance = 0   
   
   override this.Receive message reply = task {
      match message with
      | Deposit amount   -> balance <- balance + amount
      | Withdraw amount  -> balance <- balance - amount
      | Balance          -> reply balance
   }
