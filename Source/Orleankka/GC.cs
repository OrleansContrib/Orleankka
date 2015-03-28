using System;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Core;

    class GC
    {
        readonly Type actor;
        TimeSpan timeout = TimeSpan.Zero;

        public GC(Type actor)
        {
            this.actor = actor;
            
            var attribute = actor.GetCustomAttribute<KeepAliveAttribute>(inherit: true);
            if (attribute == null)
                return;

            var hours = TimeSpan.FromHours(attribute.Hours);
            var minutes = TimeSpan.FromMinutes(attribute.Minutes);
            
            SetKeepAlive(hours.Add(minutes));
        }

        public void SetKeepAlive(TimeSpan timeout)
        {
            if (timeout < TimeSpan.FromMinutes(1))
                throw new InvalidOperationException(
                    "Minimum activation GC timeout is 1 minute. Actor: " + actor);

            if (this.timeout != TimeSpan.Zero)
                throw new InvalidOperationException(
                    "Either declarative or imperative definition of keep alive timeout can be used at a time");

            this.timeout = timeout;
        }

        public void KeepAlive(IActorEndpointActivationService endpoint)
        {
            if (timeout == TimeSpan.Zero)
                return;

            endpoint.DelayDeactivation(timeout);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class KeepAliveAttribute : Attribute
    {
        public double Minutes;
        public double Hours;
    }
}
