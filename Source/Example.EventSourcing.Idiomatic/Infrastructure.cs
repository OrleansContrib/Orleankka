using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

namespace Example
{
    public abstract class CqsActor : Actor
    {
        public override Task<object> OnReceive(object message)
        {
            var cmd = message as Command;
            if (cmd != null)
                return HandleCommand(cmd);

            var query = message as Query;
            if (query != null)
                return HandleQuery(query);

            throw new InvalidOperationException("Unknown message type: " + message.GetType());
        }

        protected abstract Task<object> HandleCommand(Command cmd);
        protected abstract Task<object> HandleQuery(Query query);
    }

    public abstract class EventSourcedActor : CqsActor
    {
        protected override async Task<object> HandleCommand(Command cmd)
        {
            var events = (await Dispatch<IEnumerable<Event>>(cmd)).ToArray();

            foreach (var @event in events)
                ((dynamic)this).On((dynamic)@event);

            return (object) events;
        }

        protected override Task<object> HandleQuery(Query query)
        {
            return Dispatch(query);
        }
    }
}