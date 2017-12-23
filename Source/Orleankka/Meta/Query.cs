namespace Orleankka.Meta
{
    public interface Query : Message
    {}

    public interface Query<TResult> : Query
    {}

    public interface Query<TActor, TResult> : ActorMessage<TActor, TResult>, Query<TResult> where TActor : IActor
    {}
}