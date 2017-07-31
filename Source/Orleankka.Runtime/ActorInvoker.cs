using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorInvoker
    {
        Task<object> OnReceive(Actor actor, object message);
        Task OnReminder(Actor actor, string id);

        Task OnActivate(Actor actor);
        Task OnDeactivate(Actor actor);
    }

    public abstract class ActorInvoker : IActorInvoker
    {
        public readonly IActorInvoker Next;

        protected ActorInvoker(IActorInvoker next = null)
        {
            Next = next ?? DefaultActorInvoker.Instance;
        }

        public virtual Task<object> OnReceive(Actor actor, object message) => Next.OnReceive(actor, message);
        public virtual Task OnReminder(Actor actor, string id) => Next.OnReminder(actor, id);

        public virtual Task OnActivate(Actor actor) => Next.OnActivate(actor);
        public virtual Task OnDeactivate(Actor actor) => Next.OnDeactivate(actor);
    }

    class DefaultActorInvoker : IActorInvoker
    {
        public static readonly DefaultActorInvoker Instance = new DefaultActorInvoker();

        public Task<object> OnReceive(Actor actor, object message) => actor.OnReceive(message);
        public Task OnReminder(Actor actor, string id) => actor.OnReminder(id);

        public Task OnActivate(Actor actor) => actor.OnActivate();
        public Task OnDeactivate(Actor actor) => actor.OnDeactivate();
    }
}