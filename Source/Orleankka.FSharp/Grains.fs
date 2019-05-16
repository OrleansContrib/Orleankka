namespace Orleankka.FSharp

[<Orleankka.ActorGrainMarkerInterface>]
type IActorGrain<'TMsg> =
    inherit Orleankka.IActorGrain