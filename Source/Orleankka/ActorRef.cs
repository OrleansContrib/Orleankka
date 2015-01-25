using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorRef
    {
        Task Tell(object message);
        
        Task<TResult> Ask<TResult>(object message);

        ActorPath Path {get;}
    }

    public static class ActorRefExtensions
    {
        public static Task<object> Ask(this IActorRef @ref, object message)
        {
            return @ref.Ask<object>(message);
        }
    }

    class ActorRef : IActorRef
    {
        readonly IActor actor;

        internal ActorRef(ActorPath path, IActor actor)
        {
            Path = path;
            this.actor = actor;
        }

        public ActorPath Path
        {
            get; private set;
        }

        public Task Tell(object message)
        {
            Requires.NotNull(message, "message");

            return actor
                    .OnTell(message)
                    .UnwrapExceptions();
        }

        public async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, "message");

            return (TResult) await actor
                    .OnAsk(message)
                    .UnwrapExceptions();
        }
    }
}