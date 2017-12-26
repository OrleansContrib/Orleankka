namespace Orleankka
{
    public abstract class ObserverRef
    {
        public abstract void Notify(object message);
    }

    public abstract class ObserverRef<TActor> where TActor : IActorGrain
    {
        public abstract void Notify(ActorMessage<TActor> message);
    }
}