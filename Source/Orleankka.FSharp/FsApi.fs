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
   open Orleankka.Embedded
   open Orleankka.Playground
   
   let inline playgroundActorSystem () = ActorSystem.Configure().Playground()

   let inline register data silo =
      (^silo : (member Register : ^data -> EmbeddedConfigurator) (silo, data)) 

   let inline start (cfg : EmbeddedConfigurator) = cfg.Done()


let inline (<!) (actorRef : ActorRef) (message : obj) = actorRef.Tell message |> AsyncTask.Await