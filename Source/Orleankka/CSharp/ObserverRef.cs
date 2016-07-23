namespace Orleankka.CSharp
{
    public abstract class ObserverRef<TActor> where TActor : IActor
    {
        public abstract void Notify(ActorMessage<TActor> message);
    }
}