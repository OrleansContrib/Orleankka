
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

type ActorConfig() = 
   let mutable _name = ""
   let mutable _init = Unchecked.defaultof<Init>
   let mutable _receive = Unchecked.defaultof<Receive>

   member this.Name with get() = _name and internal set(value) = _name <- value
   member this.Init with get() = _init and internal set(value) = _init <- value
   member this.Receive with get() = _receive and internal set(value) = _receive <- value


type ActorConfigurationBuilder() =
      
   member this.Zero() = ActorConfig()

   member this.Yield(()) = this.Zero()

   [<CustomOperation("init", MaintainsVariableSpace = true)>]
   member this.Init(config : ActorConfig, init : 'TState -> 'TState) =
      config.Init <- fun state -> init(Unchecked.defaultof<'TState>) :> obj
      config

   [<CustomOperation("receive", MaintainsVariableSpace = true)>]
   member this.Receive(config : ActorConfig, receive : 'TState -> 'TMessage -> ActorContext -> Task<'TState>) =
      config.Receive <- fun (state : obj) (msg : obj) context -> 
                           (receive (state :?> 'TState) (msg :?> 'TMessage) context)
                              .ContinueWith<obj>(fun (task : Task<'TState>) -> task.Result :> obj)
      config
   

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


open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

let private serializeConfig (config : ActorConfig) =   
   let binary = FsPickler.CreateBinary()
   let serialized = <@ config @> |> binary.Pickle |> Encoding.Default.GetString
   config.Name, serialized


let private fillActorConfig configExpr =
   match configExpr with
   | PropertyGet (_, p, _) -> let config = p.GetValue(null) :?> ActorConfig
                              config.Name <- p.Name
                              config
   | _ -> failwith "Can't parse ActorConfig expression."


let registerFuncActors (configsExpr : Expr<ActorConfig list>) (silo : EmbeddedConfigurator) =             
   
   let rec loop(expr : Expr, map : Map<string, string>) =
      match expr with
      | NewUnionCase (_, h::t) -> 
         let newMap = h |> fillActorConfig |> serializeConfig |> map.Add
         loop(t.Head, newMap)
      | _ -> map

   let configs = loop(configsExpr, Map.empty)
   let dict = Dictionary(configs)
   
   silo |> System.register [|Assembly.GetExecutingAssembly()|]
        |> System.run<FuncActorBootstrap> dict


let spawn<'TState, 'TMessage> (system : IActorSystem) (config : ActorConfig) id =   
   system.ActorOf(ActorPath.From(typedefof<FuncActor>, config.Name + ":" + id))