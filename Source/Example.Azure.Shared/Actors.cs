using Orleankka;

namespace Example.Azure
{
    [ActorType("Hub")]
    public interface IHub : IActor
    {}

    [ActorType("Publisher")]
    public interface IPublisher : IActor
    {}
}