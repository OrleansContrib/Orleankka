namespace Actors

open Messages
open Orleankka
open Orleankka.FSharp
open Orleankka.FSharp.Configuration
   
open FSharpx.Task

[<ActorType("ChatUser")>]
type ChatUser() =
   inherit Actor<ChatUserMessage>()
      
   let send stream message userId =      
      stream <! { UserName = userId; Text = message } 
      
   override this.Receive message = task {      
      
      let userId = this.Id
      
      match message with
      | Join room      -> let msg = sprintf "%s joined the room %s ..." userId room
                          printfn "[server]: %s" msg
                          let stream = ActorSystem.streamOf this.System "sms" room
                          do! send stream msg userId
                          return response()
                          
      | Leave room     -> let msg = sprintf "%s left the room %s!" userId room
                          printfn "[server]: %s" msg
                          let stream = ActorSystem.streamOf this.System "sms" room
                          do! send stream msg userId
                          return response()
      
      | Say (room,msg) -> let msg = sprintf "%s said: %s" userId msg
                          printfn "[server]: %s" msg
                          let stream = ActorSystem.streamOf this.System "sms" room
                          do! send stream msg userId
                          return response()
   }