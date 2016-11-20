module Shop

open Orleankka
open Orleankka.FSharp

open Account
   
type ShopMessage =
   | Sell of Account : ActorRef<obj> * Count : int
   | CheckIn of Count : int
   | Cash
   | Stock

type Shop() =
   inherit Actor<ShopMessage>()
   
   let price = 10
   let mutable cash = 0
   let mutable stock = 0   
   
   override this.Receive message = task {
      match message with

      | CheckIn count -> stock <- stock + count   
                         return nothing
      
      | Sell (account, count) ->
         let amount = count * price
         do! account <! Withdraw(amount)
         cash <- cash + amount
         stock <- stock - count           
         return nothing                   

      | Cash  -> return response(cash)
      | Stock -> return response(stock)
   }     