using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    using Internal;

    class ActorProxy : IActorProxy
    {
        readonly IActorHost host;
        readonly ActorPath path;

        public ActorProxy(IActorHost host, ActorPath path)
        {
            this.host = host;
            this.path = path;
        }

        public Task OnTell(object message)
        {
            return host.ReceiveTell(new Request(path, message));
        }

        public async Task<object> OnAsk(object message)
        {
            return (await host.ReceiveAsk(new Request(path, message))).Message;
        }
    }
}