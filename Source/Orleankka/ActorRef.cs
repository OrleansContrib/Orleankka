using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    public interface IActorRef
    {
        Task Tell(object message);
        
        Task<object> Ask(object message);
    }

    class ActorRef : IActorRef
    {
        readonly IActor actor;

        internal ActorRef(ActorPath path)
        {
            actor = ActorFactory.Instance.GetReference(path.Type, path.Id);
        }

        public Task Tell(object message)
        {
            Requires.NotNull(message, "message");

            return actor
                    .OnTell(message)
                    .UnwrapExceptions();
        }

        public Task<object> Ask(object message)
        {
            Requires.NotNull(message, "message");

            return actor
                    .OnAsk(message)
                    .UnwrapExceptions();
        }
    }
}