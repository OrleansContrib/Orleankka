namespace Orleankka.Meta
{
    public interface Command : Message
    {}

    public interface Command<TActor> : ActorMessage<TActor>, Command where TActor : IActor
    {}
}