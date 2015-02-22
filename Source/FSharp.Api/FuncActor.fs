
module Orleankka.FSharp.FuncActor

open System.Text
open System.Reflection
open System.Collections.Generic
open System.Threading.Tasks
open Orleans
open Orleankka
open Orleankka.FSharp.Task
open Orleankka.Cluster
open Orleankka.Embedded
open FSharp.Quotations.Evaluator
open Nessos.FsPickler

type ActorContext = { 
   mutable Response : obj
}

type ActorConfig = {
   Name : string
   Receive : obj * obj * ActorContext -> Task<obj>
}

type ActorConfigurationBuilder() =
      
   member this.Zero() = {
      Name = ""
      Receive = fun (state, msg, context) -> task { return state }
   }

   member this.Yield(()) = this.Zero()

   [<CustomOperation("name", MaintainsVariableSpace = true)>]
   member this.Id(config, name) = { config with ActorConfig.Name = name }

   [<CustomOperation("receive", MaintainsVariableSpace = true)>]
   member this.Receive(config, receive : 'TState -> 'TMessage -> ActorContext -> Task<'TState>) = { 
      config with Receive = fun (state, msg, context) -> 
                              (receive (state :?> 'TState) (msg :?> 'TMessage) context)
                                 .ContinueWith<obj>(fun (task : Task<'TState>) -> task.Result :> obj)
   }

let actor = ActorConfigurationBuilder()

let private actorHandlers = Dictionary<string, FSharpFunc<obj * obj * ActorContext, Task<obj>>>()   

type private FuncActor() =
   inherit Actor()
      
   let _context = { Response = null }
   let mutable _state = null
   let mutable _handler = Unchecked.defaultof<FSharpFunc<obj * obj * ActorContext, Task<obj>>>

   override this.OnActivate() = 
      _handler <- actorHandlers.[base.Id.Split([|':'|], count = 2).[0]]
      TaskDone.Done

   override this.OnReceive(msg : obj) = task {
      _context.Response <- null
      let! newState = _handler.Invoke(_state, msg, _context)
      _state <- newState
      return _context.Response
   }

type FuncActorBootstrap() =
   inherit Bootstrapper()

   override this.Run(properties : IDictionary<string, string>) =
      let handler = properties.Item("test_actor")
      let bytes = Encoding.Default.GetBytes(handler)
      let binary = FsPickler.CreateBinary()
      let expr = binary.UnPickle<Quotations.Expr<FSharpFunc<obj * obj * ActorContext, Task<obj>>>>(bytes)
      let compiled = expr.Compile()
      actorHandlers.Add("test_actor", compiled)
      TaskDone.Done

let private serialize (actorHandler : 'TState * 'TMessage * ActorContext -> Task<'TState>) =
   let binary = FsPickler.CreateBinary()
   <@ actorHandler @> |> binary.Pickle |> Encoding.Default.GetString

let serializeConfig (config : ActorConfig) =
   let serializedHandler = config.Receive |> serialize
   config.Name, serializedHandler

let registerFuncActors (configs : ActorConfig seq) (silo : EmbeddedConfigurator) =          
   let dict = Dictionary<string, string>()         
   
   configs |> Seq.map(serializeConfig) 
            |> Seq.iter(fun (name, handler) -> dict.Add(name, handler))      
   
   silo |> System.register [|Assembly.GetExecutingAssembly()|]
         |> System.run<FuncActorBootstrap> dict

let spawn<'TState, 'TMessage> (system : IActorSystem) (config : ActorConfig) id =   
   system.ActorOf(ActorPath.From(typedefof<FuncActor>, config.Name + ":" + id))