namespace Actors

open FSharp.Control.Tasks

open Orleankka
open Orleankka.FSharp

open Messages

type ChatUser() =
    inherit ActorGrain()

    member this.send roomName message = 
        printfn "[server]: %s" message
        let room = ActorSystem.streamOf(this.System, "sms", roomName)
        room <! { UserName = this.Id; Text = message } 
    
    interface IChatUser
    override this.Receive(message) = task {      
        match message with
        | :? ChatUserMessage as m -> 
            match m with
            | Join room         ->  let msg = sprintf "%s joined the room %s ..." this.Id room
                                    do! this.send room msg
                                    return none()    
                          
            | Leave room        ->  let msg = sprintf "%s left the room %s!" this.Id room
                                    do! this.send room msg
                                    return none()

            | Say (room,msg)    ->  let msg = sprintf "%s said: %s" this.Id msg
                                    do! this.send room msg
                                    return none()

        | _ -> return unhandled()
   }