using System.Threading.Tasks;

namespace Orleankka
{
    using System;

    public interface IActorRefInvoker
    {
        Task<TResult> Send<TResult>(ActorPath actor, object message, Func<object, Task<object>> invoke);
    }

    public abstract class ActorRefInvoker : IActorRefInvoker
    {
        readonly IActorRefInvoker next;

        protected ActorRefInvoker(IActorRefInvoker next = null)
        {
            this.next = next ?? DefaultActorRefInvoker.Instance;
        }

        public virtual Task<TResult> Send<TResult>(ActorPath actor, object message, Func<object, Task<object>> invoke) => 
            next.Send<TResult>(actor, message, invoke);
    }

    class DefaultActorRefInvoker : IActorRefInvoker
    {
        public static readonly DefaultActorRefInvoker Instance = new DefaultActorRefInvoker();

        public async Task<TResult> Send<TResult>(ActorPath actor, object message, Func<object, Task<object>> invoke) => 
            (TResult) await invoke(message);
    }
}