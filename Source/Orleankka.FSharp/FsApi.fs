module Orleankka.FSharp

module Async = 
   open System.Threading.Tasks
   
   let inline AwaitVoidTask (task : Task) = 
      let continuation (t : Task) : unit = 
         match t.IsFaulted with
         | true -> raise t.Exception
         | arg -> ()
      task.ContinueWith continuation |> Async.AwaitTask
