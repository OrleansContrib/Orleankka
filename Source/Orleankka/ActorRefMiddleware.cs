using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorRefMiddleware
    {
        Task<object> Receive(ActorPath actor, object message, Receive receiver);
    }

    public abstract class ActorRefMiddleware : IActorRefMiddleware
    {
        readonly IActorRefMiddleware next;

        protected ActorRefMiddleware(IActorRefMiddleware next = null) => 
            this.next = next ?? DefaultActorRefMiddleware.Instance;

        public virtual Task<object> Receive(ActorPath actor, object message, Receive receiver) => 
            next.Receive(actor, message, receiver);
    }

    public class DefaultActorRefMiddleware : IActorRefMiddleware
    {
        public static readonly DefaultActorRefMiddleware Instance = new DefaultActorRefMiddleware();

        public Task<object> Receive(ActorPath actor, object message, Receive receiver) => 
            receiver(message);
    }
}