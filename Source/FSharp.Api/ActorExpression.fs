module ActorExpression

open System
open System.Threading.Tasks

let inline response(data:obj) = Task.FromResult(data)

let inline taskDone() = Task.FromResult(null) :> Task

module Configurations =
    type BodyConfiguration<'TMessage> = {
        OnReceive: 'TMessage -> Task<obj>
        OnReminder: (string -> Task) option
        OnActivate: (unit -> Task) option
        OnDeactivate: (unit -> Task) option
    }

    type ActorConfiguration<'TMessage> =  {
        Id: string
        KeepAlive: System.TimeSpan
        Body: unit->BodyConfiguration<'TMessage>
    }

module ActorsRegister =
    open Configurations

    let mutable private actors  = Map.empty<string,ActorConfiguration<obj>>

    let addActor actor =
        actors<- actors.Add (actor.Id,actor)

    let register ()= actors|> Map.toSeq |> Seq.map (fun (k,v)->v)

module Builders =
    open Configurations

    let emptyBody(): BodyConfiguration<'a> = {
        OnReceive = fun m -> response(null)
        OnReminder = None
        OnActivate = None
        OnDeactivate = None
    }

    let emptyConfig() : ActorConfiguration<'a> = { Id = ""; KeepAlive = TimeSpan.Zero; Body = fun () -> emptyBody() }

    type ActorBuilder<'TMessage>() =

        member __.Yield(item: 'a) : ActorConfiguration<'TMessage> = emptyConfig()

        [<CustomOperation("actorType")>]
        member __.ActorType(actor, id) = { actor with Id = id }

        [<CustomOperation("keepAlive")>]
        member __.KeepAlive(actor, timeSpan) = { actor with KeepAlive = timeSpan }

        [<CustomOperation("body")>]
        member __.Body(actor, body) =
                let a = { actor with Body = body }
                ActorsRegister.addActor a
                a

    type BodyBulder<'TMessage>() =
        member __.Zero() =  {
            OnReceive = fun m -> response(null)
            OnReminder = None
            OnActivate = None
            OnDeactivate = None
        }
        member x.Yield(()) : BodyConfiguration<'TMessage> = x.Zero()

        [<CustomOperation("onReceive")>]
        member __.OnReceive(body, receive) : BodyConfiguration<'TMessage> = {body with OnReceive = receive}

        [<CustomOperation("onReminder", MaintainsVariableSpace = true)>]
        member __.OnReminder(body, reminder: string ->Task) : BodyConfiguration<'TMessage> =
            { body with OnReminder = reminder|> Some }

        [<CustomOperation("onActivate", MaintainsVariableSpace = true)>]
        member __.OnActivate(body, activate) : BodyConfiguration<'TMessage> = { body with OnActivate = activate|> Some }

        [<CustomOperation("onDeactivate", MaintainsVariableSpace = true)>]
        member __.OnDeactivate(body, deactivate) : BodyConfiguration<'TMessage> =
            { body with OnDeactivate = deactivate |> Some }

let handlers<'a> = Builders.BodyBulder<'a>()

let actor<'a> = Builders.ActorBuilder<'a>()