module Shop

open FSharp.Control.Tasks
open Orleankka
open Orleankka.FSharp

open Account
   
type ShopMessage =
   | Sell of Account : ActorRef<obj> * Count : int
   | CheckIn of Count : int
   | Cash
   | Stock

type IShop = 
   inherit IActorGrain<ShopMessage>

type Shop() =
    inherit FsActorGrain()
   
    let price = 10
    let mutable cash = 0
    let mutable stock = 0   
   
    interface IShop
    override this.Receive(message, response) = task {
        match message with
        | :? ShopMessage as m -> 
            match m with
            | CheckIn count -> stock <- stock + count   
      
            | Sell (account, count) ->
                let amount = count * price
                do! account <! Withdraw(amount)
                cash <- cash + amount
                stock <- stock - count           

            | Cash  -> response <? cash
            | Stock -> response <? stock
        
        | _ -> response <? ActorGrain.Unhandled
   }     