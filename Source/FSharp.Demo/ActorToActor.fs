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
   | Balance 
   
type ShopMessage =
   | Sell of Account : ActorRef * Count : int
   | CheckIn of Count : int
   | Cash
   | Stock
   
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
         

type Shop() as this =
   inherit FunActor()   
   
   let price = 10

   let mutable cash = 0
   let mutable stock = 0
   
   do 
      this.Receive(fun message -> task {         
         match message with
         
         | Sell (account, count) ->
            let amount = count * price
            do! account <? Withdraw(amount)
            cash <- cash + amount
            stock <- stock - count
                     
         | CheckIn count -> stock <- stock + count

         | Cash  -> this.Reply(cash)
         | Stock -> this.Reply(stock)
      })     

let startDemo (system : IActorSystem) =
   
   let shop = system.ActorOf<Shop>("Amazon")
   let account = system.ActorOf<Account>("Antya")
   
   task {

      let! stock = shop <? Stock
      printfn "Shop has %i items in stock \n" stock

      let! balance = account <? Balance
      printfn "Account balance is %i \n" balance

      printfn "Let's put 100$ on the account \n"
      do! account <? Deposit(100)      

      printfn "Let's put 5 items in stock \n"
      do! shop <? CheckIn(5)

      let! shopItems = shop <? Stock
      printfn "Now shop has %i items in stock \n" stock

      printfn "Let's sell 2 items to user \n"
      do! shop <? Sell(account, 2)      

      let! stock = shop <? Stock
      printfn "Now shop has %i items in stock \n" stock

      let! balance = account <? Balance
      printfn "And account balance is %i \n" balance
   }