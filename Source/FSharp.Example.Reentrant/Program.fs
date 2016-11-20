open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration

open RealTimeCounter

// here we demonstrate a feature called "Reentrancy"
// more info you can find at: http://dotnet.github.io/orleans/Advanced-Concepts/Reentrant-Grains 

[<EntryPoint>]
let main argv =    
   
   printfn "Running demo. Booting cluster might take some time ...\n"

   // setup actor system
   use system = [|Assembly.GetExecutingAssembly()|]
                |> ActorSystem.createPlayground
                |> ActorSystem.start   
   
   // get uniq actor by name
   let counter = ActorSystem.actorOf<Counter>(system, "realtime-consistent-counter")

   let writeJob() = task {
      Console.ForegroundColor <- ConsoleColor.Red 
      printfn "\n send Increment message which should take 5 sec to finish. \n"
      do! counter <! Increment // this message is not reentrant which means blocking operation

      Console.ForegroundColor <- ConsoleColor.Red
      printfn "\n send Increment message which should take 5 sec to finish. \n"
      do! counter <! Increment 

      Console.ForegroundColor <- ConsoleColor.Red
      printfn "\n send Increment message which should take 5 sec to finish. \n"
      do! counter <! Increment 
   }

   let readJob() = task {      
      let mutable count = 0
      while count < 3 do         
         let! result = counter <? GetCount // this message is reentrant which means not blocking operation
         count <- result
         
         Console.ForegroundColor <- ConsoleColor.Yellow
         printfn " current value is %d" count
         do! Task.delay(TimeSpan.FromSeconds(0.5))

      Console.ForegroundColor <- ConsoleColor.Green
      printfn "\n job is finished."

   }
      
   writeJob() |> ignore
   Task.run(readJob) |> ignore

   Console.ReadLine() |> ignore
   0 // return an integer exit code
