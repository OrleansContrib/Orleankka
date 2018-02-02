namespace Orleankka
{
    public interface ActorMessage<TActor> where TActor : IActorGrain
    {}

    public interface ActorMessage<TActor, TResult> where TActor : IActorGrain
    {}
}