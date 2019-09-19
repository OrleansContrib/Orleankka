// Learn more about F# at http://fsharp.org

open Microsoft.Extensions.Logging
open Orleans
open Orleans.Configuration
open System
open System.Reflection
open FSharp.Control.Tasks.V2
open Contracts.Say
open Orleans.Hosting
open Orleankka
open Orleankka.Client
open Orleankka.FSharp

let doClientWork (system:IActorSystem) = task {
  let greeter = ActorSystem.typedActorOf<IHello, HelloMessages>(system,"greeter")
  let! hi = greeter <? Hi
  let! hello = greeter <? Hello "Roman"
  do! greeter <! Bue
  do printfn "hi: %s \nhello: %s" hi hello
}

let runMainAsync () = task {
  use codeGenLoggerFactory = new LoggerFactory();
  use client =
    ClientBuilder()
     .UseLocalhostClustering()
     .Configure(fun (x:ClusterOptions) -> x.ClusterId <- "dev";x.ServiceId <- "OrleansBasic" )
     .ConfigureLogging(fun (logging:ILoggingBuilder) -> logging.AddConsole() |> ignore)
     .ConfigureApplicationParts(fun parts -> 
        parts.AddApplicationPart(typeof<IHello>.Assembly).WithCodeGeneration(codeGenLoggerFactory) |> ignore)
     .UseOrleankka()
     .Build()
  do! client.Connect()
  Console.WriteLine("Client successfully connected to silo host \n");
  do! doClientWork (client.ActorSystem())
  do Console.ReadKey() |> ignore
}


[<EntryPoint>]
let main argv =
    // printfn "Hello World from F#!"
    runMainAsync ()
    |> Async.AwaitTask
    |> Async.RunSynchronously
    0 // return an integer exit code
