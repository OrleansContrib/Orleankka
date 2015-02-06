module Orleankka.FSharp

open System.Threading.Tasks

module AsyncTask = 
   open System.Threading.Tasks
   
   let inline Await (task : Task) = 
      let continuation (t : Task) : unit = 
         match t.IsFaulted with
         | true -> raise t.Exception
         | arg -> ()
      task.ContinueWith continuation |> Async.AwaitTask

module System =       
   open System.Reflection
   open Orleans.Runtime.Configuration   
   open Orleankka.Embedded
   open Orleankka.Playground
   
   let inline playgroundActorSystem () = ActorSystem.Configure().Playground()

   let inline register data silo =
      (^silo : (member Register : ^data -> EmbeddedConfigurator) (silo, data)) 

   let inline start (cfg : EmbeddedConfigurator) = cfg.Done()


let inline (<?) (actorRef : ActorRef) (message : obj) = actorRef.Ask<'TResponse>(message) |> Async.AwaitTask

let inline (<!) (actorRef : ActorRef) (message : obj) = actorRef.Ask<obj>(message) |> Async.AwaitTask |> Async.Ignore

[<AbstractClass>]
type Actor<'TMsg, 'TResponse>() =
   inherit Actor()
   
   abstract Receive : 'TMsg -> Async<'TResponse>

   override this.OnAsk(msg : obj) =
      match msg with
      | :? 'TMsg as m ->

         let source = TaskCompletionSource<obj>();

         async { 
            let! response = this.Receive(m)
            source.SetResult(response :> obj)
         }
         |> Async.StartImmediate

         source.Task
                  
      | _ -> failwith( sprintf "Unsupported message type %s" (msg.GetType().ToString()) )