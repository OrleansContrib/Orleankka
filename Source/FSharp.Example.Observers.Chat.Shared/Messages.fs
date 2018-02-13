module Messages

open System.Linq
open System.Collections.Generic

open Orleankka
open Orleankka.FSharp

type ClientMessage =
   | NewMessage of Username:string * Text:string
   | Notification of Text:string

type ServerMessage =
   | Join of Username:string * Client:ObserverRef
   | Say of Username:string * Text:string   
   | Disconnect of Username:string * Client:ObserverRef