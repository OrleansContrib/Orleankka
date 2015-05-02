module Shop

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.FuncActor
open Account

type ShopMessage =
   | Sell of Account:ActorRef<AccountMessage> * Count:int
   | CheckIn of Count:int
   | Cash
   | Stock

type private ShopState = { Price:int; Cash:int; Stock:int }

let ShopActor = actor {   
   init (fun shop -> { Price = 10; Cash = 0; Stock = 0 })
   receive (fun shop message context -> task {
      match message with

      | CheckIn count -> return { shop with Stock = shop.Stock + count }
      
      | Sell (account, count) ->
         let amount = count * shop.Price
         do! account <? Withdraw(amount)
         return { shop with Cash = shop.Cash + amount
                            Stock = shop.Stock - count }
      
      | Cash  -> context.Reply(shop.Cash)
                 return shop

      | Stock -> context.Reply(shop.Stock)
                 return shop
   })
}
