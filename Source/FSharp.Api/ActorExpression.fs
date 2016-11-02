module ActorExpression

open System
open System.Threading.Tasks

type LifecycleMessage =
   | Activate
   | Deactivate

type TickMessage =
   | Timer of Id:string * State:obj
   | Reminder of string

type ActorMessage =
   | LifecycleMessage
   | TickMessage

let inline response(data:obj) = Task.FromResult(data)

let inline taskDone() = Task.FromResult(null) :> Task

module Configurations =

    type ActorConfiguration =  {
        TypeName: string
        KeepAlive: System.TimeSpan option
        Body: unit -> (obj -> Task<obj>)
    }

module Builders =
    open Configurations

    let emptyConfig() : ActorConfiguration = { TypeName = ""; KeepAlive = None; Body = fun () -> fun m -> response(null) }

    type ActorBuilder() =

        member __.Yield(item: 'a) : ActorConfiguration = emptyConfig()

        [<CustomOperation("typeName")>]
        member __.ActorType(actor, typeName) = { actor with TypeName = typeName }

        [<CustomOperation("keepAlive")>]
        member __.KeepAlive(actor, timeSpan) = { actor with KeepAlive = timeSpan }

        [<CustomOperation("body")>]
        member __.Body(actor, body) = { actor with Body = body }                            
               

let actor<'a> = Builders.ActorBuilder()

module ActorRegister = 
    
    open Orleankka
    open Orleankka.FSharp

    let actorConfigs = new System.Collections.Generic.List<Configurations.ActorConfiguration>()

    let add (actor:Configurations.ActorConfiguration) =
        actorConfigs.Add(actor)
    
    type FSharpInvoker(body: obj -> Task<obj>) = 
        interface IActorInvoker with 
            member __.OnActivate () = Activate |> body :> Task
            member __.OnReceive msg = msg |> body
            member __.OnReminder id = Reminder(id) |> body :> Task
            member __.OnTimer (id,state) = Timer(id,state) |> body :> Task
            member __.OnDeactivate () = Deactivate |> body :> Task

    let toActorConfiguration (actorConfig: Configurations.ActorConfiguration ) =
        let actor = new ActorConfiguration(actorConfig.TypeName)
        actor.KeepAliveTimeout  <- match actorConfig.KeepAlive with 
                                    | Some t ->t
                                    | None -> TimeSpan.FromDays(365.*10.)

        let activator = System.Func<ActorPath,IActorRuntime,IActorInvoker>(
            fun path runtime -> actorConfig.Body() |> FSharpInvoker :> IActorInvoker
        )
        actor.Activator <- activator
        
        actor :> EndpointConfiguration


    type FSharpActorSystemConfiguratorExtention() = 
        inherit ActorSystemConfiguratorExtension()
        let configs = new System.Collections.Generic.List<EndpointConfiguration>()

        override  this.Configure(conf: IActorSystemConfigurator) = 
               conf.Register (configs.ToArray())

         member this.RegisterConfigs (config: Configurations.ActorConfiguration seq) = 
                config
                |> Seq.map toActorConfiguration 
                |> configs.AddRange


    open System.Reflection
 
    open System.Runtime.CompilerServices
    open System.Collections.Generic

    [<Extension>]
    type Ext () =
        [<Extension>]
        static member inline FSharp<'T when 'T :> IExtensibleActorSystemConfigurator> (config:'T,configure: FSharpActorSystemConfiguratorExtention ->unit)=
            System.Action<_> (configure) |>  config.Extend 
            config

//    type 'T when 'T :> string with
//        member this.FSharp()=
//            this.Extend (fun x->ignore())
//            this
//
//        member this.FSharp  (configure: FSharpActorSystemConfiguratorExtention ->unit) =
//            System.Action<_>(configure) |> this.Extend 
//            this

module RegistrationExample = 
   
   let TestActor = actor {
      body (fun()->
         fun msg -> 
            printfn "received"
            1 |> response
   )}

   open ActorRegister
   open Orleankka
   open Orleankka.Client
   open Orleankka.Playground
   open Orleankka.CSharp
    
//
//    let inline createClient config assemblies = 
//        ActorSystem.Configure().Client().From(config).CSharp(fun x -> x.Register(assemblies) |> ignore).Done()
//
//    let inline createFSharpClient config =
//        ActorSystem.Configure().Client().From(config).FSharp().Done()

   let inline createPlayground () = 
      ActorSystem.Configure().Playground().FSharp(fun x->x.RegisterConfigs( ActorRegister.actorConfigs )).Done()