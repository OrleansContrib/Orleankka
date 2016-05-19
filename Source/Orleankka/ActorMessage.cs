namespace Orleankka
{
    public interface ActorMessage<TActor> where TActor : Actor
    {}

    public interface ActorMessage<TActor, TResult> where TActor : Actor
    {}
}