using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    class ActorProxy : IActorProxy
    {
        readonly IActor actor;

        public ActorProxy(IActor actor)
        {
            this.actor = actor;
        }

        public Task OnTell(object message)
        {
            return actor.OnTell(message);
        }

        public Task<object> OnAsk(object message)
        {
            return actor.OnAsk(message);
        }
    }
}