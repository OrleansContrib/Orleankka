module ActorExpression

open System
open System.Threading.Tasks

let inline response(data:obj) = Task.FromResult(data)

let inline taskDone() = Task.FromResult(null) :> Task

module Configurations =
    type BodyConfiguration = {
        OnReceive: obj -> Task<obj>
        OnReminder: (string -> Task) option
        OnActivate: (unit -> Task) option
        OnDeactivate: (unit -> Task) option
    }

    type ActorConfiguration =  {
        Id: string
        KeepAlive: System.TimeSpan
        Body: unit->BodyConfiguration
    }

module ActorsRegister =
    open Configurations

    let mutable private actors  = Map.empty<string,ActorConfiguration>

    let addActor actor =
        actors<- actors.Add (actor.Id,actor)

    let register ()= actors|> Map.toSeq |> Seq.map (fun (k,v)->v)

module Builders =
    open Configurations

    let emptyBody(): BodyConfiguration = {
        OnReceive = fun m -> response(null)
        OnReminder = None
        OnActivate = None
        OnDeactivate = None
    }

    let emptyConfig() : ActorConfiguration = { Id = ""; KeepAlive = TimeSpan.Zero; Body = fun () -> emptyBody() }

    type ActorBuilder() =

        member __.Yield(item: 'a) : ActorConfiguration = emptyConfig()

        [<CustomOperation("actorType")>]
        member __.ActorType(actor, id) = { actor with Id = id }

        [<CustomOperation("keepAlive")>]
        member __.KeepAlive(actor, timeSpan) = { actor with KeepAlive = timeSpan }

        [<CustomOperation("body")>]
        member __.Body(actor, body) =
                let a = { actor with Body = body }
                ActorsRegister.addActor a
                a

    type BodyBulder() =
        member __.Zero() =  {
            OnReceive = fun m -> response(null)
            OnReminder = None
            OnActivate = None
            OnDeactivate = None
        }
        member x.Yield(()) : BodyConfiguration = x.Zero()

        [<CustomOperation("onReceive")>]
        member __.OnReceive(body, receive) : BodyConfiguration = {body with OnReceive = receive}

        [<CustomOperation("onReminder", MaintainsVariableSpace = true)>]
        member __.OnReminder(body, reminder: string ->Task) : BodyConfiguration =
            { body with OnReminder = reminder|> Some }

        [<CustomOperation("onActivate", MaintainsVariableSpace = true)>]
        member __.OnActivate(body, activate) : BodyConfiguration = { body with OnActivate = activate|> Some }

        [<CustomOperation("onDeactivate", MaintainsVariableSpace = true)>]
        member __.OnDeactivate(body, deactivate) : BodyConfiguration =
            { body with OnDeactivate = deactivate |> Some }

let handlers<'a> = Builders.BodyBulder()

let actor<'a> = Builders.ActorBuilder()

module ActorRegister = 
    
    open Orleankka
    
//    type FSharpActor(actor: Configurations.ActorConfiguration<'a> ) = 
//            inherit EndpointConfiguration(actor.Id)

    type FSharpInvoker(body: Configurations.BodyConfiguration) = 
        interface IActorInvoker with 
            member __.OnActivate () =  
                                            match body.OnActivate  with
                                            | Some f -> f()
                                            | None   -> taskDone()

            member __.OnReceive msg = msg |>  body.OnReceive

            member __.OnReminder reminder = 
                                            match body.OnReminder with
                                            | Some f->reminder |> f
                                            | None -> taskDone()
            member __.OnTimer (timer,state) = taskDone()
            member __.OnDeactivate () = taskDone()

    let toActorConfiguration (actorConfig: Configurations.ActorConfiguration ) =
        let actor = new ActorConfiguration(actorConfig.Id)
        actor.KeepAliveTimeout <- actorConfig.KeepAlive 

        let activator = System.Func<ActorPath,IActorRuntime,IActorInvoker>(
            fun path runtime -> actorConfig.Body() |> FSharpInvoker :> IActorInvoker
        )
        actor.Activator <- activator
        actor :> EndpointConfiguration
            
        

    type FSharpActorSystemConfiguratorExtention() = 
        inherit ActorSystemConfiguratorExtension()
        
        override  this.Configure(conf: IActorSystemConfigurator) = 
               let configs =  ActorsRegister.register()
                                |> Seq.map toActorConfiguration
                                |> Seq.toArray
               conf.Register configs  
                             

    type IExtensibleActorSystemConfigurator with
        member this.FSharp()=
            this.Extend (fun x->ignore())
            this

        member this.FSharp  (configure: FSharpActorSystemConfiguratorExtention ->unit) =
            System.Action<_>(configure) |> this.Extend 
            this

module RegistrationExample = 
    let test = actor {
        actorType "testActor"
        body (fun () ->
                    handlers{
                        onReceive (
                                fun t-> 
                                    printfn "received"
                                    1|> response
                                    )
                    }
                )
    }

    open ActorRegister
    open Orleankka
    open Orleankka.Client
    open Orleankka.Playground
    open Orleankka.CSharp
    

    let inline createClient config assemblies = 
        ActorSystem.Configure().Client().From(config).CSharp(fun x -> x.Register(assemblies) |> ignore).Done()

    let inline createFSharpClient config =
        ActorSystem.Configure().Client().From(config).FSharp(fun x -> ignore()).Done()

    let inline createPlayground ()= 
        ActorSystem.Configure().Playground().FSharp().Done()