using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Core
{
    using Static; 
    using Utility; 

    static class ActorEndpointDynamicFactory
    {
        readonly static Dictionary<Type, Func<string, object>> factories =
                    new Dictionary<Type, Func<string, object>>();

        public static IActorEndpoint Proxy(ActorPath path)
        {
            var factory = factories.Find(path.Type);

            if (factory == null)
               throw new InvalidOperationException(
                   string.Format("Type: '{0}' is not registered as an Actor or Worker", path.Type));

            return (IActorEndpoint)factory(path.ToString());
        }

        public static void Reset()
        {
            factories.Clear();
        }

        public static void Register(Type type)
        {
            var actor  = type.GetCustomAttribute<ActorAttribute>();
            var worker = type.GetCustomAttribute<WorkerAttribute>();

            if (actor != null && worker != null)
                throw new InvalidOperationException("A type cannot be configured to be both Actor and Worker: " + type);

            factories.Add(type, worker != null 
                                    ? GetWorkerFactory() 
                                    : GetActorFactory(actor));
        }

        static Func<string, object> GetWorkerFactory()
        {
            return WFactory.GetGrain;
        }

        static Func<string, object> GetActorFactory(ActorAttribute actor)
        {
            if (actor == null)
                actor = new ActorAttribute();

            switch (actor.Placement)
            {
                case Placement.Random:
                    return A0Factory.GetGrain;
                case Placement.PreferLocal:
                    return A1Factory.GetGrain;
                case Placement.DistributeEvenly:
                    return A2Factory.GetGrain;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}