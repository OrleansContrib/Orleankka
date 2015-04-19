using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka.Meta;
using Orleankka.Typed;

namespace Example
{
    public abstract class EventSourcedActor : TypedActor
    {
        protected override async Task<object> OnInvoke(MemberInfo member, object[] arguments)
        {
            var r = await base.OnInvoke(member, arguments);
            
            var result = r as IEnumerable<Event>;
            if (result == null)
                return r;

            var events = result.ToArray();
            foreach (var @event in events)
                Dispatch(@event);

            return events;
        }
    }
}