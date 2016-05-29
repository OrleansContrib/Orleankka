using System;

using Orleans;

namespace Orleankka.Core
{
    using Endpoints;

    class ActorInterface
    {
        internal static ActorInterface From(Type interfaceType, Type implementationType)
        {
            return new ActorInterface(interfaceType, implementationType, Bind(interfaceType));
        }

        static Func<string, object> Bind(Type type)
        {
            var method = typeof(GrainFactory).GetMethod("GetGrain", new[] { typeof(string), typeof(string) });
            var invoker = method.MakeGenericMethod(type);
            var instance = Activator.CreateInstance(typeof(GrainFactory), nonPublic: true);
            return x => invoker.Invoke(instance, new object[] {x, null});
        }

        readonly Reentrant reentrant;
        readonly Func<string, object> factory;

        ActorInterface(Type interfaceType, Type implementationType, Func<string, object> factory)
        {
            Type = interfaceType;
            this.factory = factory;
            reentrant = new Reentrant(implementationType);
        }

        public Type Type { get; private set; }

        internal bool IsReentrant(object message) => reentrant.IsReentrant(message);
        internal IActorEndpoint Proxy(ActorPath path) => (IActorEndpoint)factory(path.Serialize());        
    }
}