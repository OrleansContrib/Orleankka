using Orleans;

namespace Orleankka
{
    [GenerateSerializer]
    public abstract class ObserverRef
    {
        public abstract void Notify(object message);
    }

    [GenerateSerializer]
    public abstract class ObserverRef<TActor> where TActor : IActorGrain, IGrainWithStringKey
    {
        public abstract void Notify(ActorMessage<TActor> message);
    }
}