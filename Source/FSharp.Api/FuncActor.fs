
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

type private Init = obj -> obj
type private Receive = obj -> obj -> ActorContext -> Task<obj>

type ActorConfig = {
   Name : string
   Init : Init
   Receive : Receive
}

type ActorConfigurationBuilder() =
      
   member this.Zero() = {
      Name = ""     
      Init = fun state -> state       
      Receive = fun state msg context -> task { return state }
   }

   member this.Yield(()) = this.Zero()

   [<CustomOperation("name", MaintainsVariableSpace = true)>]
   member this.Id(config, name) = { config with ActorConfig.Name = name }

   [<CustomOperation("init", MaintainsVariableSpace = true)>]
   member this.Init(config, init : 'TState -> 'TState) = { 
      config with Init = fun state -> init(Unchecked.defaultof<'TState>) :> obj }

   [<CustomOperation("receive", MaintainsVariableSpace = true)>]
   member this.Receive(config, receive : 'TState -> 'TMessage -> ActorContext -> Task<'TState>) = { 
      config with Receive = fun state msg context -> 
                              (receive (state :?> 'TState) (msg :?> 'TMessage) context)
                                 .ContinueWith<obj>(fun (task : Task<'TState>) -> task.Result :> obj)
   }

let actor = ActorConfigurationBuilder()

let private actorConfigs = Dictionary<string, ActorConfig>()   

type private FuncActor() =
   inherit Actor()
      
   let _context = { Response = null }
   let mutable _state = null
   let mutable _receive = Unchecked.defaultof<Receive>

   override this.OnActivate() = 
      let config = actorConfigs.[base.Id.Split([|':'|], count = 2).[0]]
      _state <- config.Init(_state)
      _receive <- config.Receive
      TaskDone.Done

   override this.OnReceive(msg : obj) = task {
      _context.Response <- null
      let! newState = _receive _state msg _context
      _state <- newState
      return _context.Response
   }

type FuncActorBootstrap() =
   inherit Bootstrapper()

   let compileExpr (handler : string) =
      let bytes = Encoding.Default.GetBytes(handler)
      let binary = FsPickler.CreateBinary()
      let expr = binary.UnPickle<Quotations.Expr<ActorConfig>>(bytes)
      expr.Compile()

   override this.Run(properties : IDictionary<string, string>) =      
      properties |> Seq.filter(fun p -> p.Key <> "<-::Type::->")
                 |> Seq.iter (fun p -> actorConfigs.Add(p.Key, compileExpr(p.Value)))
      TaskDone.Done

let private serializeConfig (config : ActorConfig) =   
   let binary = FsPickler.CreateBinary()
   let serialized = <@ config @> |> binary.Pickle |> Encoding.Default.GetString
   config.Name, serialized

let registerFuncActors (configs : ActorConfig seq) (silo : EmbeddedConfigurator) =          
   let dict = Dictionary<string, string>()         
   
   configs |> Seq.map(serializeConfig) 
           |> Seq.iter(fun (name, handler) -> dict.Add(name, handler))      
   
   silo |> System.register [|Assembly.GetExecutingAssembly()|]
        |> System.run<FuncActorBootstrap> dict

let spawn<'TState, 'TMessage> (system : IActorSystem) (config : ActorConfig) id =   
   system.ActorOf(ActorPath.From(typedefof<FuncActor>, config.Name + ":" + id))