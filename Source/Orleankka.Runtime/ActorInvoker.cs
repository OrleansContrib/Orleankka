using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorInvoker
    {
        Task<object> OnReceive(ActorGrain actor, object message);
        Task OnReminder(ActorGrain actor, string id);

        Task OnActivate(ActorGrain actor);
        Task OnDeactivate(ActorGrain actor);
    }

    public abstract class ActorInvoker : IActorInvoker
    {
        public readonly IActorInvoker Next;

        protected ActorInvoker(IActorInvoker next = null)
        {
            Next = next ?? DefaultActorInvoker.Instance;
        }

        public virtual Task<object> OnReceive(ActorGrain actor, object message) => Next.OnReceive(actor, message);
        public virtual Task OnReminder(ActorGrain actor, string id) => Next.OnReminder(actor, id);

        public virtual Task OnActivate(ActorGrain actor) => Next.OnActivate(actor);
        public virtual Task OnDeactivate(ActorGrain actor) => Next.OnDeactivate(actor);
    }

    class DefaultActorInvoker : IActorInvoker
    {
        public static readonly DefaultActorInvoker Instance = new DefaultActorInvoker();

        public Task<object> OnReceive(ActorGrain actor, object message) => actor.OnReceive(message);
        public Task OnReminder(ActorGrain actor, string id) => actor.OnReminder(id);

        public Task OnActivate(ActorGrain actor) => actor.OnActivate();
        public Task OnDeactivate(ActorGrain actor) => actor.OnDeactivate();
    }
}