
open Orleans.Runtime.Configuration
open Orleankka
open Orleankka.Client
open Orleankka.FSharp
open System
open System.Reflection
open ChatServer

let mutable client = null 

[<EntryPoint>]
let main argv =    

   let assembly = Assembly.GetExecutingAssembly()

   let config = ClientConfiguration().LoadFromEmbeddedResource(assembly, "Client.xml")
   
   use system = ActorSystem.Configure()
                           .Client()
                           .From(config)
                           .Register(typedefof<ChatServer>.Assembly)
                           .Done()

   client <- Observer.Create().Result
   let clientRef = Observer.op_Implicit(client)

   let server = system.ActorOf<ChatServer>("server")

   printfn "Enter your user name... \n"
   let userName = Console.ReadLine()     
   
   use observer = client.Subscribe(fun message ->          
      match (message :?> ClientMessage) with
      | NewMessage (userName, text) -> printfn "%s: %s\n" userName text
      | Notification text -> printfn "%s\n" text)   
   
   task {
      printfn "Connecting.... \n"      
      let! response = server <? Join(userName, clientRef)
      printfn "Connected! \n"
      printfn "%s\n" response
      
      let textStream = Seq.initInfinite(fun _ -> Console.ReadLine())
      
      textStream 
         |> Seq.find(function "quit" -> server <* Disconnect(userName, clientRef)
                                        true 
                              | text -> server <* Say(userName, text)
                                        false)
         |> ignore       
   } 
   |> Task.wait   
   
   0
