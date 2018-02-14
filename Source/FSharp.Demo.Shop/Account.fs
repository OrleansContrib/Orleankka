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
    inherit FsActorGrain()

    let mutable balance = 0   
    
    interface IAccount
    override this.Receive(message, response) = task {
        match message with
        | :? AccountMessage as m -> 
            match m with
            | Balance         -> response <? balance
            
            | Deposit amount  -> balance <- balance + amount
            
            | Withdraw amount -> if balance >= amount then balance <- balance - amount         
                                 else invalidOp "Amount may not be larger than account balance. \n"

        | _ -> response <? ActorGrain.Unhandled
    }