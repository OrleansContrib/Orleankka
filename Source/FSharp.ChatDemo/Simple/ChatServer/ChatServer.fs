module ChatServer

open Orleankka
open Orleankka.FSharp
open System.Collections.Generic
open System.Linq

type ClientMessage =
   | NewMessage of Username:string * Text:string
   | Notification of Text:string

type ServerMessage =
   | Join of Username:string * Client:ObserverRef
   | Say of Username:string * Text:string   
   | Disconnect of Username:string * Client:ObserverRef


type ChatServer() = 
   inherit Actor<ServerMessage>()

   let _users = Dictionary<string, IObserverCollection>()

   let notifyClients msg = _users.Values |> Seq.iter(fun clients -> clients <* msg)

   override this.Receive message reply = task {
      match message with
      
      | Join (userName, client) -> 
         match _users.TryGetValue(userName) with         
         | (true, clients) -> clients.Add(client)         
         | _ -> Notification(sprintf "User: %s connected..." userName) |> notifyClients
                let clients = ObserverCollection() :> IObserverCollection
                clients.Add(client)
                _users.Add(userName, clients)
                         
         reply "Hello and welcome to Orleankka chat example"                              
      
      | Say (userName, text) -> NewMessage(userName, text) |> notifyClients
      
      | Disconnect (userName, client) -> 
         match _users.TryGetValue(userName) with
         | (true, clients) -> if clients.Count() = 1 then _users.Remove(userName) |> ignore
                              else clients.Remove(client)
         | _ -> ()
   }