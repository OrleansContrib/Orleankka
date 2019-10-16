namespace Grains

open Orleankka
open Orleankka.FSharp
open Contracts.Say
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2
module Say =

  type HelloGrain (loggerFactory:ILoggerFactory) =
    inherit ActorGrain()
    let log = loggerFactory.CreateLogger(typeof<HelloGrain>)
        
    interface IHello
    override this.Receive(msg) = task {
      match msg with
      | :? HelloMessages as m ->
        match m with
        | Hi -> 
          log.LogInformation("Client asked to say Hi!")
          return some "Oh, hi!"
        | Hello s ->
          log.LogInformation (sprintf "Client asked to say Hello to %s" s)
          return sprintf "Hello, %s" s |> some
        | Bue -> 
          log.LogInformation ("Client wants to go")
          return none()
      | _ ->  return unhandled()
    }