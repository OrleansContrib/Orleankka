namespace Orleankka.Meta
{
    public interface Command
    {}

    public interface Command<TActor> : ActorMessage<TActor> where TActor : IActor
    {}
}