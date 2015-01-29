open System
open System.Reflection
open Orleans
open Orleankka
open Orleans.Runtime.Configuration
open Orleankka.FSharp

type Message = 
   | Greet of string
   | Hi

type GreetingActor() = 
   inherit Actor()   

   override this.OnTell(message : obj) =
      this.Handle(message :?> Message)      

   member this.Handle(message) =            
      
      match message with
      | Greet who -> printfn "Hello %s" who
      | Hi -> printfn "Hello from F#!"            
      
      TaskDone.Done

[<EntryPoint>]
let main argv = 
   let assembly = Assembly.GetExecutingAssembly()
   
   let serverConfig = ServerConfiguration().LoadFromEmbeddedResource(assembly, "Orleans.Server.Configuration.xml")
   let clientConfig = ClientConfiguration().LoadFromEmbeddedResource(assembly, "Orleans.Client.Configuration.xml")

   let silo = EmbeddedSilo().With(serverConfig).With(clientConfig).Start()

   let actorSystem = ActorSystem.Instance

   let actor = actorSystem.ActorOf<GreetingActor>(Guid.NewGuid().ToString())

   async {
      do! actor.Tell( Hi ) |> Async.AwaitVoidTask
      do! actor.Tell( Greet("Yevhen") ) |> Async.AwaitVoidTask
      do! actor.Tell( Greet("AntyaDev") ) |> Async.AwaitVoidTask
   }
   |> Async.RunSynchronously

   Console.ReadLine() |> ignore
   
   silo.Dispose()

   printfn "%A" argv
   0 // return an integer exit code
