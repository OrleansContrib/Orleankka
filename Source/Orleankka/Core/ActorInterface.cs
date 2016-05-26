using System;
using System.Reflection;

using Orleans;

namespace Orleankka.Core
{
    using Endpoints;

    class ActorInterface
    {
        internal static ActorInterface From(Type actor)
        {
            return new ActorInterface(actor);
        }

        readonly Reentrant reentrant;
        Func<string, object> factory;

        ActorInterface(Type actor)
        {
            reentrant = new Reentrant(actor);
        }

        public Type Type { get; private set; }

        string Generate()
        {
            return "";
        }

        internal void Bind(string code, Assembly assembly)
        {
            Type type = assembly.GetType($"I{code}Endpoint");
        }

        Func<string, object> MakeFactory(Type type)
        {
            var method = typeof(GrainFactory).GetMethod("GetGrain", new[] { typeof(string), typeof(string) });
            var invoker = method.MakeGenericMethod(type);
            var instance = Activator.CreateInstance(typeof(GrainFactory), nonPublic: true);
            return x => invoker.Invoke(instance, new object[] {x, null});
        }

        internal bool IsReentrant(object message) => reentrant.IsReentrant(message);
        internal IActorEndpoint Proxy(ActorPath path) => (IActorEndpoint)factory(path.Serialize());        
    }
}