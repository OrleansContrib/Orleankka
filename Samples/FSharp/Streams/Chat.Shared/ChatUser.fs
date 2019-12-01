namespace Messages

open Orleans
open Orleankka.FSharp
   
type ChatUserMessage =
   | Join  of room:string
   | Leave of room:string
   | Say   of room:string * message:string

type IChatUser =
   inherit IGrainWithStringKey 
   inherit IActorGrain<ChatUserMessage>
