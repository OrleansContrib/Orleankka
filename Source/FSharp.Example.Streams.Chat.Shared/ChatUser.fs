namespace Messages

open Orleankka.FSharp
   
type ChatUserMessage =
   | Join  of room:string
   | Leave of room:string
   | Say   of room:string * message:string

type IChatUser =
   inherit IActorGrain<ChatUserMessage>
