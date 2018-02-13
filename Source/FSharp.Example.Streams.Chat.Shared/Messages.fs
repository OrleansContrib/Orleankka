namespace Messages

open System
   
type ChatRoomMessage = {
   UserName:string
   Text:string
}

type ChatUserMessage =
   | Join  of room:string
   | Leave of room:string
   | Say   of room:string * message:string
