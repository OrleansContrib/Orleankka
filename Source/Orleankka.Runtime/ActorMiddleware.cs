using System;
using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorMiddleware
    {
        Task<object> Receive(ActorGrain actor, object message, Func<object, Task<object>> receiver);
    }

    public abstract class ActorMiddleware : IActorMiddleware
    {
        public readonly IActorMiddleware Next;

        protected ActorMiddleware(IActorMiddleware next = null) => 
            Next = next ?? DefaultActorMiddleware.Instance;

        public abstract Task<object> Receive(ActorGrain actor, object message, Func<object, Task<object>> receiver);
    }

    class DefaultActorMiddleware : IActorMiddleware
    {
        public static readonly DefaultActorMiddleware Instance = new DefaultActorMiddleware();

        public Task<object> Receive(ActorGrain actor, object message, Func<object, Task<object>> receiver) => 
            receiver(message);
    }
}