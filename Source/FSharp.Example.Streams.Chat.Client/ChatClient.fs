module Client.ChatClient

open Orleankka
open Orleankka.FSharp
open Messages

open FSharpx
open FSharpx.Task


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

   do! client.User.Tell <| Join(client.RoomName)

   return { client with Subscription = Some sb }
}

let leave (client:ChatClient) = task {
   do! unsubscribe client.Subscription.Value
   do! client.User.Tell <|  Leave(client.RoomName)
}

let say (client:ChatClient) (message:string) = task {
   do! client.User.Tell <| Say(client.RoomName, message)   
}