module Orleankka.FSharp

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
   open Orleankka.Configuration.Embedded
   open Orleankka.Configuration.Playground
   
   let inline playgroundActorSystem () = ActorSystem.Configure().Playground()

   let inline register data silo =
      (^silo : (member Register : ^data -> ActorSystemEmbeddedConfiguration) (silo, data)) 

   let inline start (cfg : ActorSystemEmbeddedConfiguration) = cfg.Done()


let inline (<!) (actorRef : ActorRef) (message : obj) = actorRef.Tell message |> AsyncTask.Await