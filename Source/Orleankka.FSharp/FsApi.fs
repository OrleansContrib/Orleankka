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
   
   let inline createSilo () = EmbeddedSilo()

   let inline configWith config silo = 
      (^silo : (member With : ^config -> EmbeddedSilo) (silo, config))   

   let inline registerWith data silo =
      (^silo : (member Register : ^data -> EmbeddedSilo) (silo, data)) 

   let inline start (silo : EmbeddedSilo) = silo.Start()


let inline (<!) (actorRef : ActorRef) (message : obj) = actorRef.Tell message |> AsyncTask.Await