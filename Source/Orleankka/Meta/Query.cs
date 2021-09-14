using Orleans;

namespace Orleankka.Meta
{
    public interface Query : Message
    {}

    public interface Query<TResult> : Query, Message<TResult>
    {}

    public interface Query<TActor, TResult> : ActorMessage<TActor, TResult>, Query<TResult> where TActor : IActorGrain, IGrainWithStringKey
    {}
}