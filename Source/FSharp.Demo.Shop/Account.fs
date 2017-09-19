module Account

open Orleankka
open Orleankka
open Orleankka.FSharp

type AccountMessage = 
   | Balance
   | Deposit of int
   | Withdraw of int 

type Account() = 
   inherit Actor<AccountMessage>()

   let mutable balance = 0   
   
   override this.Receive message = task {
      match message with
      | Balance         -> return response(balance)
      
      | Deposit amount  -> balance <- balance + amount
                           return nothing
      | Withdraw amount -> 
          if balance >= amount then balance <- balance - amount         
          else invalidOp "Amount may not be larger than account balance. \n"
          return nothing
   }