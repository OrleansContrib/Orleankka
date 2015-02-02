open System
open System.Reflection
open Orleans
open Orleankka
open Orleans.Runtime.Configuration
open Orleankka.FSharp
open Orleankka.FSharp.System

type Message = 
   | Greet of string
   | Hi

type GreetingActor() = 
   inherit Actor()   

   override this.OnTell(message : obj) =      
      match message with
      | :? Message as m ->
                   
         match m with
         | Greet who -> printfn "Hello %s" who
         | Hi -> printfn "Hello from F#!"     
         TaskDone.Done            

      | _ -> failwith "unknown message"

type ProxyActor() = 
   inherit Actor()   

   override this.OnTell(message : obj) =      
      match message with
      | :? Message as m ->
                   
         match m with
         | Greet who -> this.System.ActorOf<GreetingActor>("test").Tell(m)
         | Hi -> this.System.ActorOf<GreetingActor>("test").Tell(m)
         
      | _ -> failwith "unknown message"


[<EntryPoint>]
let main argv = 
   let assembly = Assembly.GetExecutingAssembly()
   
   let serverConfig = ServerConfiguration().LoadFromEmbeddedResource(assembly, "Orleans.Server.Configuration.xml")
   let clientConfig = ClientConfiguration().LoadFromEmbeddedResource(assembly, "Orleans.Client.Configuration.xml")

   use silo = createSilo()
              |> configWith serverConfig
              |> configWith clientConfig
              |> registerWith [|assembly|]
              |> start

   let actorSystem = ActorSystem.Instance

   let actor = actorSystem.ActorOf<ProxyActor>(Guid.NewGuid().ToString())

   async {
      do! actor <! Hi
      do! actor <! Greet "Yevhen"
      do! actor <! Greet "AntyaDev"
   }
   |> Async.RunSynchronously

   Console.ReadLine() |> ignore

   printfn "%A" argv
   0 // return an integer exit code
