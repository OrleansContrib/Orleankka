module Shop

open FSharp.Control.Tasks

open Orleans
open Orleankka
open Orleankka.FSharp

open Account

type ShopMessage =
   | Sell of Account : ActorRef<obj> * Count : int
   | CheckIn of Count : int
   | Cash
   | Stock

type IShop = 
   inherit IGrainWithStringKey
   inherit IActorGrain<ShopMessage>

type Shop() =
    inherit ActorGrain()
   
    let price = 10
    let mutable cash = 0
    let mutable stock = 0   
   
    interface IShop
    override this.Receive(message) = task {
        match message with
        | :? ShopMessage as m -> 
            match m with
            | CheckIn count -> stock <- stock + count   
                               return none()
      
            | Sell (account, count) ->
                let amount = count * price
                do! account <! Withdraw(amount)
                cash <- cash + amount
                stock <- stock - count           
                return none()

            | Cash  -> return some(cash)
            | Stock -> return some(stock)
        
        | _ -> return unhandled()
   }     