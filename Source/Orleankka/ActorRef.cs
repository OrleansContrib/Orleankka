using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    public class ActorRef : IEquatable<ActorPath>
    {
        readonly IActor actor;

        protected ActorRef()
        {}

        internal ActorRef(ActorPath path, IActor actor)
        {
            Path = path;
            this.actor = actor;
        }

        public ActorPath Path
        {
            get; private set;
        }

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, "message");

            return actor
                    .OnTell(message)
                    .UnwrapExceptions();
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, "message");

            return (TResult) await actor
                    .OnAsk(message)
                    .UnwrapExceptions();
        }

        public static implicit operator ActorPath(ActorRef arg)
        {
            return arg.Path;
        }

        bool IEquatable<ActorPath>.Equals(ActorPath other)
        {
            return Path.Equals(other);
        }
    }

    public static class ActorRefExtensions
    {
        public static Task<object> Ask(this ActorRef @ref, object message)
        {
            return @ref.Ask<object>(message);
        }
    }
}