namespace Messages

open Orleankka

type IChatUser = 
   inherit IActor

type ChatRoomMessage = {
   UserName:string
   Text:string
}

type ChatUserMessage =
   | Join  of room:string
   | Leave of room:string
   | Say   of room:string * message:string