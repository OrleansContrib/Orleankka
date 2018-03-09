module Client.ChatClient

open FSharp.Control.Tasks
open Orleankka
open Orleankka.FSharp
open Messages

type ChatClient = {
   UserName: string
   User: ActorRef<ChatUserMessage>
   RoomName: string   
   Room: StreamRef<ChatRoomMessage>   
   Subscription: Option<StreamSubscription>
}

let join (client:ChatClient) = task {   
   let! sb = client.Room.Subscribe (fun message ->
      if message.UserName <> client.UserName then printfn "%s" message.Text
   )

   do! client.User <! Join(client.RoomName)
   return { client with Subscription = Some sb }
}

let leave (client:ChatClient) = task {
   do! client.Subscription.Value.Unsubscribe()
   do! client.User <! Leave(client.RoomName)
}

let say (client:ChatClient) (message:string) = task {
   do! client.User <! Say(client.RoomName, message)   
}