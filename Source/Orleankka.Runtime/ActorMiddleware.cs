using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorMiddleware
    {
        Task<object> Receive(ActorGrain actor, object message, Receive receiver);
    }

    public abstract class ActorMiddleware : IActorMiddleware
    {
        readonly IActorMiddleware next;

        protected ActorMiddleware(IActorMiddleware next = null) => 
            this.next = next ?? DefaultActorMiddleware.Instance;

        public virtual Task<object> Receive(ActorGrain actor, object message, Receive receiver) =>
            next.Receive(actor, message, receiver);
    }

    class DefaultActorMiddleware : IActorMiddleware
    {
        public static readonly DefaultActorMiddleware Instance = new DefaultActorMiddleware();

        public Task<object> Receive(ActorGrain actor, object message, Receive receiver) => 
            receiver(message);
    }
}