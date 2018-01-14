using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka
{
    public interface IActorInvoker
    {
        Task<object> ReceiveRequest(ActorGrain actor, object message);
    }

    public abstract class ActorInvoker : IActorInvoker
    {
        public readonly IActorInvoker Next;

        protected ActorInvoker(IActorInvoker next = null) => 
            Next = next ?? DefaultActorInvoker.Instance;

        public virtual Task<object> ReceiveRequest(ActorGrain actor, object message) => 
            Next.ReceiveRequest(actor, message);
    }

    class DefaultActorInvoker : IActorInvoker
    {
        public static readonly DefaultActorInvoker Instance = new DefaultActorInvoker();

        public Task<object> ReceiveRequest(ActorGrain actor, object message) => 
            actor.Receive(message);
    }
}