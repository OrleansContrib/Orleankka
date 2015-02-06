module ActorToActor

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
   | GetBalance 
   
type ShopMessage =
   | Sell of ActorRef * int
   | ItemsCount
   | MoneyBalance
   

type BankAccount() =
   inherit Actor<AccountMessage, int>()

   let mutable _balance = 0

   override this.Receive(msg) = async {     
                   
      match msg with         
      | Deposit a -> _balance <- _balance + a
      | Withdraw a -> _balance <- _balance - a
      | GetBalance -> ()
      
      return _balance
   }

type Shop() =
   inherit Actor<ShopMessage, int>()

   let mutable _money = 0
   let mutable _items = 0

   override this.Receive(msg) = async {      
                        
      match msg with
      | Sell (account, count) ->
         
         let money = _items * 10

         let! balance = account <? Withdraw(money)

         _money <- _money + balance
         _items <- _items - count
         return count
      
      | ItemsCount -> return _items
      | MoneyBalance -> return _money
   }

let startDemo (system : IActorSystem) =
   
   let shop = system.ActorOf<Shop>("shop")
   let userTest = system.ActorOf<BankAccount>("user test")   

   async {
      let! balance = userTest <? Deposit(100)      
      printf "user's balance is %i" balance
      do! shop <! Sell(userTest, 5)      
   }
   |> Async.RunSynchronously