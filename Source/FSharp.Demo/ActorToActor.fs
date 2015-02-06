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
   | Sell of Account : ActorRef * Items : int
   | PutItems of Items : int
   | GetItems
   | GetMoney
   

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
      | Sell (account, items) ->
         
         let money = _items * 10

         let! balance = account <? Withdraw(money)

         _money <- _money + balance
         _items <- _items - items
         return _items
      
      | PutItems items ->
         _items <- _items + items
         return _items

      | GetItems -> return _items
      | GetMoney -> return _money
   }

let startDemo (system : IActorSystem) =
   
   let shop = system.ActorOf<Shop>("shop")
   let userTest = system.ActorOf<BankAccount>("user test")   

   async {   
      
      let! shopItems = shop <? GetItems      
      printfn "shop's items count is %i \n" shopItems

      let! balance = userTest <? GetBalance
      printfn "user's balance is %i \n" balance

      printfn "let's put 100$ deposit on user's account \n"
      let! balance = userTest <? Deposit(100)      
      printfn "current user's balance is %i \n" balance

      printfn "let's put new 5 items to shop \n"
      do! shop <! PutItems(5)            

      let! shopItems = shop <? GetItems      
      printfn "shop's items count is %i \n" shopItems

      printfn "let's sell 2 items to user \n"
      do! shop <! Sell(userTest, 2)      

      let! shopItems = shop <? GetItems      
      printfn "now shop's items count is %i \n" shopItems

      let! balance = userTest <? GetBalance
      printfn "current user's balance is %i \n" balance

   }
   |> Async.RunSynchronously