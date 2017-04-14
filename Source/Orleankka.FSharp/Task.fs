namespace Orleankka.FSharp

open System
open System.Threading.Tasks
open FSharpx.Task

module Task = 

   let inline awaitTask (t:Task) =
      let tcs = TaskCompletionSource()
      t.ContinueWith(fun t ->
         match t.IsFaulted with
         | false -> if t.IsCanceled then tcs.SetCanceled()
                    else tcs.SetResult()
         | true  -> tcs.SetException(t.Exception.GetBaseException())) |> ignore
      tcs.Task
      
   let inline delay (delay:TimeSpan) = 
      let tcs = TaskCompletionSource()
      Task.Delay(delay).ContinueWith(fun _ -> tcs.SetResult()) |> ignore
      tcs.Task   

   let completedTask = () |> returnM