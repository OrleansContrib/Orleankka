module Demo

open System
open System.Reflection

open Orleankka
open Orleankka.Playground
open Orleankka.FSharp

open Shop
open Account

[<EntryPoint>]
let main argv = 

   printfn "Running demo. Booting cluster might take some time ...\n"
   
   use system = 
    ActorSystem
     .Configure()
     .Playground()
     .Assemblies([|Assembly.GetExecutingAssembly()|])
     .Done()
                     
   let job() = task {
      do! Task.awaitTask(system.Start())
  
      let shop = ActorSystem.actorOf<Shop>(system, "amazon")
      let account = ActorSystem.actorOf<Account>(system, "antya")

      let! stock = shop <? Stock
      printfn "Shop has %i items in stock \n" stock

      let! balance = account <? Balance
      printfn "Account balance is %i \n" balance

      printfn "Let's put 100$ on the account \n"
      do! account <! Deposit(100)      

      printfn "Let's put 5 items in stock \n"
      do! shop <! CheckIn(5)

      let! stock = shop <? Stock
      printfn "Now shop has %i items in stock \n" stock

      try
         printfn "Let's sell 100 items to user \n"
         do! shop <! Sell(account, 100)
      with :? InvalidOperationException as e -> printf "[Exception]: %s \n" e.Message

      printfn "Let's sell 2 items to user \n"
      do! shop <! Sell(account, 2)      

      let! stock = shop <? Stock
      printfn "Now shop has %i items in stock \n" stock

      let! balance = account <? Balance
      printfn "And account balance is %i \n" balance
   }

   Task.run(job) |> ignore

   Console.ReadLine() |> ignore
   0