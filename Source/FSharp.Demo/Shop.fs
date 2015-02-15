module Shop

open Orleankka
open Orleankka.FSharp

open Account
   
type ShopMessage =
   | Sell of Account : ActorRef * Count : int
   | CheckIn of Count : int
   | Cash
   | Stock

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