
[<AutoOpen>]
module Orleankka.FSharp.Task

open System
open System.Threading
open System.Threading.Tasks


let inline wait (task : Task<_>) = task.Wait()

let toAsync (t: Task<'T>): Async<'T> =
   let abegin (cb: AsyncCallback, state: obj) : IAsyncResult = 
      match cb with
      | null -> upcast t
      | cb -> 
            t.ContinueWith(fun (_ : Task<_>) -> cb.Invoke t) |> ignore
            upcast t
   let aend (r: IAsyncResult) = 
      (r :?> Task<'T>).Result
   Async.FromBeginEnd(abegin, aend)

/// Transforms a Task's first value by using a specified mapping function.
let inline mapWithOptions (token: CancellationToken) (continuationOptions: TaskContinuationOptions) (scheduler: TaskScheduler) f (m: Task<_>) =
   m.ContinueWith((fun (t: Task<_>) -> f t.Result), token, continuationOptions, scheduler)

/// Transforms a Task's first value by using a specified mapping function.
let inline map f (m: Task<_>) =
   m.ContinueWith(fun (t: Task<_>) -> f t.Result)

let inline bindWithOptions (token: CancellationToken) (continuationOptions: TaskContinuationOptions) (scheduler: TaskScheduler) (f: 'T -> Task<'U>) (m: Task<'T>) =
   m.ContinueWith((fun (x: Task<_>) -> f x.Result), token, continuationOptions, scheduler).Unwrap()

let inline bind (f: 'T -> Task<'U>) (m: Task<'T>) = 
   m.ContinueWith(fun (x: Task<_>) -> f x.Result).Unwrap()

let inline returnM a = 
   let s = TaskCompletionSource()
   s.SetResult a
   s.Task

let inline whenAll f (tasks : Task<_> seq) = Task.WhenAll(tasks) |> map(f)

/// Sequentially compose two actions, passing any value produced by the first as an argument to the second.
let inline (>>=) m f = bind f m

/// Flipped >>=
let inline (=<<) f m = bind f m

/// Sequentially compose two either actions, discarding any value produced by the first
let inline (>>.) m1 m2 = m1 >>= (fun _ -> m2)

/// Left-to-right Kleisli composition
let inline (>=>) f g = fun x -> f x >>= g

let inline private flip f a b = f b a

let inline private konst a _ = a

/// Right-to-left Kleisli composition
let inline (<=<) x = flip (>=>) x

/// Promote a function to a monad/applicative, scanning the monadic/applicative arguments from left to right.
let inline lift2 f a b = 
   a >>= fun aa -> b >>= fun bb -> f aa bb |> returnM

/// Sequential application
let inline ap x f = lift2 id f x

/// Sequential application
let inline (<*>) f x = ap x f

/// Infix map
let inline (<!>) f x = map f x

/// Sequence actions, discarding the value of the first argument.
let inline ( *>) a b = lift2 (fun _ z -> z) a b

/// Sequence actions, discarding the value of the second argument.
let inline ( <*) a b = lift2 (fun z _ -> z) a b
    
type TaskBuilder(?continuationOptions, ?scheduler, ?cancellationToken) =
   let contOptions = defaultArg continuationOptions TaskContinuationOptions.None
   let scheduler = defaultArg scheduler TaskScheduler.Default
   let cancellationToken = defaultArg cancellationToken CancellationToken.None

   member this.Return x = returnM x

   member this.Zero() = returnM ()

   member this.ReturnFrom (a: Task<'T>) = a

   member this.Bind(m, f) = bindWithOptions cancellationToken contOptions scheduler f m

   member this.Combine(comp1, comp2) =
      this.Bind(comp1, comp2)

   member this.While(guard, m) =
      if not(guard()) then this.Zero() else
            this.Bind(m(), fun () -> this.While(guard, m))

   member this.TryWith(t:unit -> Task<_>, catchFn:exn -> Task<_>) =
      t().ContinueWith(fun (t:Task<_>) -> 
            match t.IsFaulted with
            | false -> returnM(t.Result)
            | true  -> catchFn(t.Exception))
         .Unwrap()

   member this.TryFinally(m, compensation) =
      try this.ReturnFrom m
      finally compensation()

   member this.Using(res: #IDisposable, body: #IDisposable -> Task<_>) =
      this.TryFinally(body res, fun () -> match res with null -> () | disp -> disp.Dispose())

   member this.For(sequence: seq<_>, body) =
      this.Using(sequence.GetEnumerator(),
                           fun enum -> this.While(enum.MoveNext, fun () -> body enum.Current))

   member this.Delay (f: unit -> Task<'T>) = f

   member this.Run (f: unit -> Task<'T>) = f()

type TaskBuilderWithToken(?continuationOptions, ?scheduler) =
   let contOptions = defaultArg continuationOptions TaskContinuationOptions.None
   let scheduler = defaultArg scheduler TaskScheduler.Default

   let lift (t: Task<_>) = fun (_: CancellationToken) -> t
   let bind (t: CancellationToken -> Task<'T>) (f: 'T -> (CancellationToken -> Task<'U>)) =
      fun (token: CancellationToken) ->
            (t token).ContinueWith((fun (x: Task<_>) -> f x.Result token), token, contOptions, scheduler).Unwrap()
        
   member this.Return x = lift (returnM x)

   member this.ReturnFrom t = lift t

   member this.ReturnFrom (t: CancellationToken -> Task<'T>) = t

   member this.Zero() = this.Return ()

   member this.Bind(t, f) = bind t f            

   member this.Bind(t, f) = bind (lift t) f                

   member this.Combine(t1, t2) = bind t1 (konst t2)        

   member this.While(guard, m) =
            if not(guard()) then 
               this.Zero()
            else
               bind m (fun () -> this.While(guard, m))                    

   member this.TryFinally(t : CancellationToken -> Task<'T>, compensation) =
      try t
      finally compensation()

   member this.Using(res: #IDisposable, body: #IDisposable -> (CancellationToken -> Task<'T>)) =
      this.TryFinally(body res, fun () -> match res with null -> () | disp -> disp.Dispose())

   member this.For(sequence: seq<'T>, body) =            
            this.Using(sequence.GetEnumerator(),
                           fun enum -> this.While(enum.MoveNext, fun token -> body enum.Current token))
        
   member this.Delay f = this.Bind(this.Return (), f)


let task = TaskBuilder(scheduler = TaskScheduler.Current)