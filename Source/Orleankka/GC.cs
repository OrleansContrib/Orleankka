using System;
using System.Reflection;

namespace Orleankka
{
    using Core;

    class GC
    {
        readonly TimeSpan timeout = TimeSpan.Zero;

        public GC(Type actor)
        {
            var attribute = actor.GetCustomAttribute<KeepAliveAttribute>(inherit: true);
            if (attribute == null)
                return;

            timeout = TimeSpan.FromHours(attribute.Hours)
                 .Add(TimeSpan.FromMinutes(attribute.Minutes));

            if (timeout < TimeSpan.FromMinutes(1))
                throw new InvalidOperationException(
                    "Minimum activation GC timeout is 1 minute. Actor: " + actor);
        }

        public void KeepAlive(ActorEndpoint endpoint)
        {
            if (timeout == TimeSpan.Zero)
                return;

            endpoint.DelayDeactivation(timeout);
        }
    }
}
