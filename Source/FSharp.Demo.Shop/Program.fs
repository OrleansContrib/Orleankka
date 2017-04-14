open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration
open Orleankka.FSharp.Runtime
open Shop
open Account

open FSharpx
open FSharpx.Task

[<EntryPoint>]
let main argv = 

   printfn "Running demo. Booting cluster might take some time ...\n"
   
   let system = [|Assembly.GetExecutingAssembly()|]
                |> ActorSystem.createPlayground
                |> ActorSystem.start
                  
   let shop = ActorSystem.actorOf<Shop>(system, "amazon")
   let account = ActorSystem.actorOf<Account>(system, "antya")
   
   let job() = task {
      let! stock = shop.Ask <|Stock
      printfn "Shop has %i items in stock \n" stock

      let! balance = account.Ask <|Balance
      printfn "Account balance is %i \n" balance

      printfn "Let's put 100$ on the account \n"
      do! account.Tell <|Deposit(100)      

      printfn "Let's put 5 items in stock \n"
      do! shop.Tell <|CheckIn(5)

      let! stock = shop.Ask <|Stock
      printfn "Now shop has %i items in stock \n" stock

      try
         printfn "Let's sell 100 items to user \n"
         do! shop.Tell <|Sell(account, 100)
      with :? InvalidOperationException as e -> printf "[Exception]: %s \n" e.Message

      printfn "Let's sell 2 items to user \n"
      do! shop.Tell <|Sell(account, 2)      

      let! stock = shop.Ask <|Stock
      printfn "Now shop has %i items in stock \n" stock

      let! balance = account.Ask <|Balance
      printfn "And account balance is %i \n" balance
   }

   Task.run(job) |> ignore

   Console.ReadLine() |> ignore
   ActorSystem.stop(system)
   0