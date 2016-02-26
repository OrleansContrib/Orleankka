namespace Orleankka.Meta
{
    public interface Command
    {}

    public interface Command<TActor> : ActorMessage<TActor>, Command where TActor : IActor
    {}
}