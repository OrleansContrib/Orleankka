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
   

type BankAccount() as this = 
   inherit FuncActor<int>()

   do 
      this.InitState(0)

      this.Receive(fun balance message -> task {         
         match message with
         | Deposit value -> return balance + value         
         | Withdraw value -> return balance - value         
         | GetBalance -> 
            this.Reply(balance)
            return balance
      })         
         

type ShopBalance = {
   Money : int;
   Items : int 
}

type Shop() as this =
   inherit FuncActor<ShopBalance>()   
   
   do 
      this.InitState({ Money = 0; Items = 0 })

      this.Receive(fun shopBalance message -> task {         
         match message with
         | Sell (account, items) ->
            let money = items * 10     
            do! account <? Withdraw(money)
            return { Money = shopBalance.Money + money
                     Items = shopBalance.Items - items }
         
         | PutItems items -> return { shopBalance with Items = items + shopBalance.Items }         
         | GetItems -> 
            this.Reply(shopBalance.Items)
            return shopBalance
         | GetMoney -> 
            this.Reply(shopBalance.Money)
            return shopBalance
      })


let startDemo (system : IActorSystem) =
   
   let shop = system.ActorOf<Shop>("shop")
   let userTest = system.ActorOf<BankAccount>("user test")
   
   task {

      let! shopItems = shop <? GetItems      
      printfn "shop's items count is %i \n" shopItems

      let! balance = userTest <? GetBalance
      printfn "user's balance is %i \n" balance

      printfn "let's put 100$ deposit on user's account \n"
      do! userTest <? Deposit(100)      
      //printfn "current user's balance is %i \n" balance

      printfn "let's put new 5 items to shop \n"
      do! shop <? PutItems(5)

      let! shopItems = shop <? GetItems      
      printfn "shop's items count is %i \n" shopItems

      printfn "let's sell 2 items to user \n"
      do! shop <? Sell(userTest, 2)      

      let! shopItems = shop <? GetItems      
      printfn "now shop's items count is %i \n" shopItems

      let! balance = userTest <? GetBalance
      printfn "current user's balance is %i \n" balance

   }