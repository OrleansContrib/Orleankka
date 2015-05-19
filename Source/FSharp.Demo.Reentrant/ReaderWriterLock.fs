module ReaderWriterLock

open Orleankka
open Orleankka.FSharp

type ReadWriteMsg =
   | Write of value:int
   | Read

type ReadWriterActor() as this =
   inherit Actor<ReadWriteMsg>()

   let mutable _state = 0

   do this.Reentrant(function 
      | Write value -> false 
      | Read        -> true)
      
   override this.Receive msg reply = task {
      match msg with
      | Write value -> _state <- value
      | Read        -> reply _state      
   }
