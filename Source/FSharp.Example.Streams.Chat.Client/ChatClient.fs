module Client.ChatClient

open Orleankka
open Orleankka.FSharp
open Messages

type ChatClient = {
   UserName: string
   User: ActorRef<obj>
   RoomName: string   
   Room: StreamRef<ChatRoomMessage>   
   Subscription: Option<StreamSubscription>
}

let join (client:ChatClient) = task {   
   let! sb = client.Room.Subscribe (fun messge ->
      if messge.UserName <> client.UserName then printfn "%s" messge.Text
   )

   do! client.User <! Join(client.RoomName)

   return { client with Subscription = Some sb }
}

let leave (client:ChatClient) = task {
   do! client.Subscription.Value.Unsubscribe() |> Task.awaitTask
   do! client.User <! Leave(client.RoomName)
}

let say (client:ChatClient) (message:string) = task {
   do! client.User <! Say(client.RoomName, message)   
}