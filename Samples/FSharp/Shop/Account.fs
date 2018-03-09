module Account

open FSharp.Control.Tasks
open Orleankka
open Orleankka.FSharp

type AccountMessage = 
   | Balance
   | Deposit of int
   | Withdraw of int 

type IAccount = 
   inherit IActorGrain<AccountMessage>

type Account() = 
    inherit ActorGrain()

    let mutable balance = 0   
    
    interface IAccount
    override this.Receive(message) = task {
        match message with
        | :? AccountMessage as m -> 
            match m with
            | Balance         -> return some(balance)
            
            | Deposit amount  -> balance <- balance + amount
                                 return none()
            
            | Withdraw amount -> if balance >= amount then balance <- balance - amount         
                                 else invalidOp "Amount may not be larger than account balance. \n"
                                 return none()

        | _ -> return unhandled()
    }
