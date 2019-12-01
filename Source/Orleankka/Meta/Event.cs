using Orleans;

namespace Orleankka.Meta
{
    public interface Event : Message
    {}

    public interface Event<TActor> : ActorMessage<TActor>, Event where TActor : IActorGrain, IGrainWithStringKey
    {}
}