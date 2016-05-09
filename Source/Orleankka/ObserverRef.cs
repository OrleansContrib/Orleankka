namespace Orleankka
{
    public abstract class ObserverRef : Ref
    {
        public abstract void Notify(object message);
    }

    public abstract class ObserverRef<TActor> : Ref where TActor : IActor
    {
        public abstract void Notify(ActorMessage<TActor> message);
    }
}