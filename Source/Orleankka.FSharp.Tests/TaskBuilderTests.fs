module Orleankka.FSharp.Tests.TaskBuilderTests

open NUnit.Framework
open System.Threading
open System.Threading.Tasks
open Orleankka.FSharp

[<Test>]
let ``task should return the right value after let!``() =
    let task = Task.TaskBuilder()
    let t() = 
        task {
            let! v = Task.Factory.StartNew(fun () -> 100)
            return v
        }

    match Task.run t with        
    | Choice1Of2 r -> Assert.AreEqual(100, r)
    | Choice2Of2 e -> Assert.Fail("Task should have been successful, but errored with exception {0}", e)


[<Test>]
let ``task should return the right value after return!``() =
    let task = Task.TaskBuilder()
    let t() = 
        task {
            return! Task.Factory.StartNew(fun () -> "hello world")
        }

    match Task.run t with
    | Choice1Of2 r -> Assert.AreEqual("hello world", r)
    | Choice2Of2 e -> Assert.Fail("Task should have been successful, but errored with exception {0}", e)


[<Test>]
let ``exception in task``() =    
    let task = Task.TaskBuilder(continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
    let t() = 
        task {
            failwith "error"
        }
    match Task.run t with    
    | Choice2Of2 e -> Assert.AreEqual("error", e.Message)
    | _ -> Assert.Fail "task should have errored"
    

[<Test>]
let ``canceled task``() =
    use cts = new CancellationTokenSource()
    let task = Task.TaskBuilder(cancellationToken = cts.Token)
    let t() = 
        task {
            cts.Token.ThrowIfCancellationRequested()
        }
    cts.Cancel()
    match Task.run t with
    | Choice2Of2 (:? System.OperationCanceledException as ex) -> ()
    | Choice2Of2 e -> Assert.Fail("Task should have been canceled, but errored with exception {0}", e)
    | Choice1Of2 r -> Assert.Fail("Task should have been canceled, but succeeded with result {0}", r)


[<Test>]
let ``canceled task 2``() =
    use cts = new CancellationTokenSource()
    let task = Task.TaskBuilder(cancellationToken = cts.Token)
    let t() = 
        task {
            let! v = Task.Factory.StartNew(fun () -> 0)
            return ()
        }
    cts.Cancel()
    match Task.run t with
    | Choice2Of2 (:? System.AggregateException as ex) -> 
      match ex.InnerException with 
      | :? TaskCanceledException -> ()
      | _ -> Assert.Fail("Task should have been canceled, but errored with exception {0}", ex)
    | Choice2Of2 e -> Assert.Fail("Task should have been canceled, but errored with exception {0}", e)
    | Choice1Of2 r -> Assert.Fail("Task should have been canceled, but succeeded with result {0}", r)


[<Test>]
let ``while``() = 
    let i = ref 10        
    let t() =
        task {
            while !i > 0 do
                decr i
                do! Task.Factory.StartNew ignore
        }
    Task.run t |> ignore
    Assert.AreEqual(0, !i)


[<Test>]
let ``try with should catch exception in the body``() =   
   let result = task {
      try 
         failwith "exception"
         return 1
      with e -> return 5
   }
   Assert.AreEqual(5, result.Result)


[<Test>]
let ``try with should catch exception in the continuation``() =   
   let result = task {
      try 
         do! Task.Factory.StartNew(fun () -> failwith "exception")            
         return 1
      with e -> return 5
   }
   Assert.AreEqual(5, result.Result)


[<Test>]
let ``try with should catch exception only by type``() =   
   let result = task {
      try 
         invalidArg "param name" "msg"
         return 1
      with                   
      | :? System.NullReferenceException -> return 5
      | :? System.ArgumentException -> return 10
      | e -> return 15
   }
   Assert.AreEqual(10, result.Result)


[<Test>]
let ``try with should do unwrapping of exception to original type if it was raised in continuation``() =   
   let result = task {
      try 
         do! Task.Factory.StartNew(fun () -> invalidArg "param name" "msg")
         return 1
      with
      | :? System.NullReferenceException -> return 5
      | :? System.ArgumentException -> return 10
      | e -> return 15
   }
   Assert.AreEqual(10, result.Result)

[<Test>]
let ``try finally should execute finally block``() =
   let mutable a = 0      
   let t = task {      
      try
         return 10
      finally
         a <- 100
   }
   t.Result |> ignore
   Assert.AreEqual(100, a)

[<Test>]
let ``try finally should exec finally block after the body task has been completed``() =
   let mutable s = ""
   
   let t = task {      
      try         
         s <- s + "1"
         do! Task.delay(System.TimeSpan.FromSeconds(1.0))
         s <- s + "3"
      finally
         s <- s + "2"
   }
   t.Result |> ignore
   Assert.AreEqual(s, "132")

[<Test>]
let ``try finally should exec finally block even if exception is occured``() =
   let mutable s = ""
   
   let t = task {      
      try         
         s <- s + "1"
         do! Task.delay(System.TimeSpan.FromSeconds(1.0))
         failwith("test finally exception")
         s <- s + "3"
      finally
         s <- s + "2"
   }

   try t.Result |> ignore
   with e -> ()

   Assert.AreEqual(s, "12")

[<Test>]
let ``try finally - finally shouldn't be invoked 2 times``() =
   let mutable s = ""
   
   let t = task {      
      try
         s <- "1"                  
      finally
         s <- s + "1"
         failwith("test finally exception")         
   }

   try t.Result |> ignore
   with e -> ()

   Assert.AreEqual(s, "11")

