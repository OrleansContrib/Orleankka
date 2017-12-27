module Demo

open System

open Orleankka
open Orleankka.FSharp
open Orleankka.Playground

type Message = 
   | Greet of string
   | Hi

type IGreeter = 
   inherit IActorGrain<Message>

type Greeter() = 
   inherit ActorGrain<Message>()
   interface IGreeter

   override this.Receive message = task {
      match message with
        | Greet who -> printfn "Hello %s" who
                       return response()
        | Hi        -> printfn "Hello from F#!"
                       return response()
   }

[<EntryPoint>]
let main argv = 

   printfn "Running demo. Booting cluster might take some time ...\n"
   
   use system = 
    ActorSystem
     .Configure()
     .Playground()
     .Assemblies([|typedefof<Greeter>.Assembly|])
     .Done()
   
   system.Start().Wait()
   
   // without this invocation the call to ActorSystem.actorOf<IGreeter> fails
   // misery, since exactly the same C# code works with dynamic invocation
   let grain = system.Client.Client.GetGrain<IGreeter>("id")
   grain.ReceiveVoid(Hi).Wait()

   let job() = task {
      let actor = ActorSystem.actorOf<IGreeter>(system, "actor_id")
      
      do! actor <! Hi
      do! actor <! Greet "Yevhen"
      do! actor <! Greet "AntyaDev"      
   }
    
   Task.run(job) |> ignore
    
   Console.ReadLine() |> ignore 
   0