namespace Orleankka.FSharp

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
