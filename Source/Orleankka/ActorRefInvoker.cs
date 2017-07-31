using System.Threading.Tasks;

namespace Orleankka
{
    using System;

    public interface IActorRefInvoker
    {
        Task Tell(ActorPath actor, object message, Func<object, Task> invoke);
        Task<TResult> Ask<TResult>(ActorPath actor, object message, Func<object, Task<object>> invoke);
        void Notify(ActorPath actor, object message, Action<object> invoke);
    }

    public abstract class ActorRefInvoker : IActorRefInvoker
    {
        public readonly IActorRefInvoker Next;

        protected ActorRefInvoker(IActorRefInvoker next = null)
        {
            Next = next ?? DefaultActorRefInvoker.Instance;
        }

        public virtual Task Tell(ActorPath actor, object message, Func<object, Task> invoke) =>
            Next.Tell(actor, message, invoke);

        public virtual Task<TResult> Ask<TResult>(ActorPath actor, object message, Func<object, Task<object>> invoke) =>
            Next.Ask<TResult>(actor, message, invoke);

        public virtual void Notify(ActorPath actor, object message, Action<object> invoke) =>
            Next.Notify(actor, message, invoke);
    }

    class DefaultActorRefInvoker : IActorRefInvoker
    {
        public static readonly DefaultActorRefInvoker Instance = new DefaultActorRefInvoker();

        public Task Tell(ActorPath actor, object message, Func<object, Task> invoke) => 
            invoke(message);

        public async Task<TResult> Ask<TResult>(ActorPath actor, object message, Func<object, Task<object>> invoke) => 
            (TResult)await invoke(message);

        public void Notify(ActorPath actor, object message, Action<object> invoke) => 
            invoke(message);
    }
}