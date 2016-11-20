open System
open System.Reflection

open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration

open ChatServer

let mutable client = null 

[<EntryPoint>]
let main argv =    
   printfn "Please wait until Chat Server has completed boot and then press enter. \n"
   Console.ReadLine() |> ignore
   
   let assembly = Assembly.GetExecutingAssembly()   

   let config = ClientConfig.loadFromResource assembly "Client.xml"
      
   use system = [|typeof<ChatServer>.Assembly|]
                |> ActorSystem.createClient config
                |> ActorSystem.conect   

   client <- ClientObservable.create().Result

   let server = ActorSystem.actorOf<ChatServer>(system, "server")

   printfn "Enter your user name... \n"
   let userName = Console.ReadLine()     
   
   use observer = client.Subscribe(fun message  ->          
      match message with
      | NewMessage (userName, text) -> printfn "%s: %s\n" userName text
      | Notification text -> printfn "%s\n" text)   
   
   let job() = task {
      printfn "Connecting.... \n"      
      let! response = server <? Join(userName, client.Ref)
      printfn "Connected! \n"
      printfn "%s\n" response
      
      let textStream = Seq.initInfinite(fun _ -> Console.ReadLine())
      
      textStream 
         |> Seq.find(function "quit" -> server <* Disconnect(userName, client.Ref)
                                        true 
                              | text -> server <* Say(userName, text)
                                        false)
         |> ignore       
   }
   
   Task.run(job) |> ignore
   
   0