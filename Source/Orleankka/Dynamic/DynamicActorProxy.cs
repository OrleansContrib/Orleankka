using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka.Dynamic.Internal;

namespace Orleankka.Dynamic
{
    class DynamicActorProxy : IActorProxy
    {
        readonly IDynamicActor actor;
        readonly ActorPath path;

        public DynamicActorProxy(IDynamicActor actor, ActorPath path)
        {
            this.actor = actor;
            this.path = path;
        }

        public Task OnTell(object message)
        {
            return actor.OnTell(new DynamicRequest(path, message));
        }

        public async Task<object> OnAsk(object message)
        {
            return (await actor.OnAsk(new DynamicRequest(path, message))).Message;
        }
    }
}