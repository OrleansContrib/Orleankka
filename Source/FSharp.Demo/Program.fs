open System
open System.Reflection
open Orleans
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.System

type Message = 
   | Greet of string
   | Hi

type Greeter() = 
   inherit Actor()   

   override this.OnTell(message : obj) =      
      match message with
      | :? Message as m ->
                   
         match m with
         | Greet who -> printfn "Hello %s" who
         | Hi -> printfn "Hello from F#!"     
         TaskDone.Done            

      | _ -> failwith "unknown message"

[<EntryPoint>]
let main argv = 

   printfn "Running demo. Booting cluster might take some time ...\n"

   let assembly = Assembly.GetExecutingAssembly()
   
   use system = playgroundActorSystem()
              |> register [|assembly|]
              |> start

   //ActorToActor.startDemo(system).Wait()

   // todo: add task builder fsharp
   
//   let actor = system.ActorOf<Greeter>(Guid.NewGuid().ToString())
//
//   async {
//      do! actor <! Hi
//      do! actor <! Greet "Yevhen"
//      do! actor <! Greet "AntyaDev"
//   }
//   |> Async.RunSynchronously

   Console.ReadLine() |> ignore

   printfn "%A" argv
   0 // return an integer exit code
