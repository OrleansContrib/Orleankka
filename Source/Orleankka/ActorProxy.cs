using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    using Internal;

    class ActorProxy : IActorProxy
    {
        readonly IActor actor;
        readonly ActorPath path;

        public ActorProxy(IActor actor, ActorPath path)
        {
            this.actor = actor;
            this.path = path;
        }

        public Task OnTell(object message)
        {
            return actor.OnTell(new Request(path, message));
        }

        public async Task<object> OnAsk(object message)
        {
            return (await actor.OnAsk(new Request(path, message))).Message;
        }
    }
}