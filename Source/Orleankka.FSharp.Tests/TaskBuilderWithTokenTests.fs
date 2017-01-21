module Orleankka.FSharp.Tests.TaskBuilderWithTokenTests

open NUnit.Framework
open System.Threading
open System.Threading.Tasks
open Orleankka.FSharp

open FSharpx.Task

let checkSuccess (expected: 'a) (t: CancellationToken -> Task<'a>) =
    match run (fun () -> (t CancellationToken.None)) with
    | Successful r -> Assert.AreEqual(expected, r)
    | Error e -> Assert.Fail("Task should have been successful, but errored with exception {0}", e)
    | Canceled _ -> Assert.Fail()


let assertCancelled (cts: CancellationTokenSource) (t: CancellationToken -> Task<'a>) = 
    match run (fun () -> t cts.Token) with
    | Error (:? System.OperationCanceledException as ex) -> ()
    | Error (:? System.AggregateException as ex) -> 
      match ex.InnerException with 
      | :? TaskCanceledException -> ()
      | _ -> Assert.Fail("Task should have been canceled, but errored with exception {0}", ex)
    | Error e -> Assert.Fail("Task should have been canceled, but errored with exception {0}", e)
    | Successful r -> Assert.Fail("Task should have been canceled, but succeeded with result {0}", r)

let checkCancelledWithToken (cts: CancellationTokenSource) (t: CancellationToken -> Task<'a>) =
    cts.Cancel()
    assertCancelled cts t

let checkCancelled (t: CancellationToken -> Task<'a>) =
    use cts = new CancellationTokenSource()
    checkCancelledWithToken cts t

[<Test>]
let ``task should return the right value after let!``() =    
    let t = 
        task {
            let! v = Task.Factory.StartNew(fun () -> 100)
            return v
        }
    checkSuccess 100 t

[<Test>]
let ``task should return the right value after return!``() =    
    let t =
        task' {
            return! Task.Factory.StartNew(fun () -> "hello world")
        }    
    checkSuccess "hello world" t

[<Test>]
let ``exception in task``() =
    let task' = Task.TaskBuilderWithToken(continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
    let t = 
        task' {
            failwith "error"
        }
    match Task.run (fun () -> t CancellationToken.None) with
    | Choice2Of2 e -> Assert.AreEqual("error", e.InnerException.Message)
    | _ -> Assert.Fail "task should have errored"

[<Test>]
let ``canceled task``() =    
    let cts = new CancellationTokenSource()
    let t = 
        task' {
            cts.Token.ThrowIfCancellationRequested()
        }
    checkCancelledWithToken cts t

[<Test>]
let ``canceled task 2``() =        
    let t = 
        task' {
            let! v = Task.Factory.StartNew(fun () -> 0)
            return ()
        }
    checkCancelled t

[<Test>]
let ``return should return value``() =        
    let t = 
        task' {            
            return 100
        }
    checkSuccess 100 t  

[<Test>]
let ``return! should accept task parametrized by CancellationToken``() =    
    let t1 = 
        task' {
            let! v = Task.Factory.StartNew(fun () -> 100)
            return v
        }

    let t2 = 
        task' {            
            return! t1
        }
    checkSuccess 100 t2    

[<Test>]
let ``bind should chain two tasks``() =    
    let t = 
        task' {
            let! v1 = Task.Factory.StartNew(fun () -> 100)
            let! v2 = Task.Factory.StartNew(fun () -> v1 + 1)
            return v2
        }    
    checkSuccess 101 t

[<Test>]
let ``bind should pass cancellation token``() =
    let i = ref 0    
    let cts = new CancellationTokenSource()
    let t = 
        task' {
            let! v1 = Task.Factory.StartNew(fun () -> 1)
            let body2 x = 
                incr i
                cts.Cancel()
                x
            let! v2 = Task.Factory.StartNew(fun () -> body2 v1)
            let body3 x =
                incr i
                x + 1
            let! v3 = Task.Factory.StartNew(fun () -> body3 v2)
            return v3
        }
    
    assertCancelled cts t    
    Assert.AreEqual(1, !i)


[<Test>]
let ``bind should chain two tasks parametrized by CancellationToken``() =    
    let t1 = 
        task' {
            return! Task.Factory.StartNew(fun () -> 100)
        }
    let t2 = 
        task' {
            return! Task.Factory.StartNew(fun () -> 200)
        }
    let t = 
        task' {
            let! v1 = t1
            let! v2 = t2
            return v1 + v2
        }
    checkSuccess 300 t

[<Test>]
let ``task should be delayed``() =
    let i = ref 0    
    let t = 
        task' {
            let body() =
                incr i
                "hello world"
            let! v = Task.Factory.StartNew(body)
            return v
        }    
    Assert.AreEqual(0, !i)
    checkSuccess "hello world" t
    Assert.AreEqual(1, !i)

let whileExpression (i: ref<int>) = 
    let task' = Task.TaskBuilderWithToken(continuationOptions = TaskContinuationOptions.ExecuteSynchronously)    
    task' {
        while !i > 0 do
            decr i
            do! Task.Factory.StartNew ignore
    }

[<Test>]
let ``while``() =
    let i = ref 10        
    let t = whileExpression i
    Task.run (fun () -> t CancellationToken.None)  |> ignore
    Assert.AreEqual(0, !i)


[<Test>]
let ``cancel while``() = 
    let i = ref 10
    let t = whileExpression i    
    checkCancelled t
    Assert.AreEqual(10, !i)

[<Test>]
let ``try with should catch exception in the body``() =
   let cts = new CancellationTokenSource()   
   let t = task' {
      try 
         failwith "exception"
         return 1
      with e -> return 5
   }
   Assert.AreEqual(5, t(cts.Token).Result)


[<Test>]
let ``try with should catch exception in the continuation``() =
   let cts = new CancellationTokenSource()   
   let t = task' {
      try 
         do! Task.Factory.StartNew(fun () -> failwith "exception")            
         return 1
      with e -> return 5
   }
   Assert.AreEqual(5, t(cts.Token).Result)


[<Test>]
let ``try with should catch exception only by type``() =   
   let cts = new CancellationTokenSource()   
   let t = task' {
      try 
         invalidArg "param name" "msg"
         return 1
      with                   
      | :? System.NullReferenceException -> return 5
      | :? System.ArgumentException -> return 10
      | e -> return 15
   }
   Assert.AreEqual(10, t(cts.Token).Result)


[<Test>]
let ``try with should do unwrapping of exception to original type if it was raised in continuation``() =   
   let cts = new CancellationTokenSource()   
   let t = task' {
      try 
         do! Task.Factory.StartNew(fun () -> invalidArg "param name" "msg")
         return 1
      with
      | :? System.NullReferenceException -> return 5
      | :? System.ArgumentException -> return 10
      | e -> return 15
   }
   Assert.AreEqual(10, t(cts.Token).Result)

let tryFinallyExpression (i: ref<int>) =     
    task' {
        let! v1 = Task.Factory.StartNew(fun () -> 100)
        try
            let! v2 = Task.Factory.StartNew(fun () -> v1 + 1)
            return v2
        finally
            incr i
    }

[<Test>]
let ``try finally should execute finally block``() =
    let i = ref 0    
    let t = tryFinallyExpression i
    checkSuccess 101 t
    Assert.AreEqual(1, !i)    

[<Test>]
let ``try finally should be cancellable``() =
    let i = ref 0    
    let t = tryFinallyExpression i    
    checkCancelled t 
    Assert.AreEqual(0, !i)

[<Test>]
let ``try finally should exec finally block after the body task has been completed``() =
   let mutable s = ""
   let cts = new CancellationTokenSource()

   let t = task' {      
      try         
         s <- s + "1"
         do! Task.delay(System.TimeSpan.FromSeconds(1.0))
         s <- s + "3"
      finally
         s <- s + "2"
   }
   t(cts.Token).Result |> ignore
   Assert.AreEqual(s, "132")

[<Test>]
let ``try finally should exec finally block even if exception is occured``() =
   let mutable s = ""
   let cts = new CancellationTokenSource()
   
   let t = task' {      
      try         
         s <- s + "1"
         do! Task.delay(System.TimeSpan.FromSeconds(1.0))
         failwith("test finally exception")
         s <- s + "3"
      finally
         s <- s + "2"
   }

   try t(cts.Token).Result |> ignore
   with e -> ()

   Assert.AreEqual(s, "12")

[<Test>]
let ``try finally - finally shouldn't be invoked 2 times``() =
   let cts = new CancellationTokenSource()
   let mutable s = ""
   
   let t = task' {      
      try
         s <- "1"                  
      finally
         s <- s + "1"
         failwith("test finally exception")         
   }

   try t(cts.Token).Result |> ignore
   with e -> ()

   Assert.AreEqual(s, "11")

[<Test>]
let ``for``() =
    let i = ref 0
    let s = [1; 2; 3; 4]
    let task' = Task.TaskBuilderWithToken(continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
    let t =
        task' {
            for x in s do
                i := !i + x
                do! Task.Factory.StartNew ignore
        }
    Task.run (fun () -> t CancellationToken.None) |> ignore
    Assert.AreEqual(10, !i)

[<Test>]
let ``combine``() =
    let flag = ref false    
    let t =
        task' {
            if true then flag := true             
            return! Task.Factory.StartNew(fun () -> "hello world")
        }
    
    checkSuccess "hello world" t
    Assert.AreEqual(true, !flag)