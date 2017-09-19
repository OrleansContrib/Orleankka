using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

namespace FSM.Infrastructure
{
    public abstract class CqsActor : Actor, IActor
    {
        public override Task<object> OnReceive(object message)
        {
            var cmd = message as Command;
            if (cmd != null)
                return HandleCommand(async () => await base.OnReceive(cmd));

            var query = message as Query;
            if (query != null)
                return HandleQuery(async () => await base.OnReceive(query));

            throw new InvalidOperationException("Unknown message type: " + message.GetType());
        }

        protected abstract Task<object> HandleCommand(Func<Task<object>> commandHandler);

        protected abstract Task<object> HandleQuery(Func<Task<object>> queryHandler);
    }
}