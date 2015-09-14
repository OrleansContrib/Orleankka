using System;
using System.Reflection;
using System.Collections.Generic;

using Orleans;

namespace Orleankka.Core
{
    using Endpoints;
    using Utility;

    static class ActorEndpointFactory
    {
        readonly static Dictionary<ActorType, Func<string, object>> factories =
                    new Dictionary<ActorType, Func<string, object>>();

        public static IActorEndpoint Proxy(ActorPath path)
        {
            var type = ActorType.Registered(path.Code);

            var factory = factories.Find(type);
            if (factory == null)
               throw new InvalidOperationException(
                   $"Path '{path}' is not registered as an Actor or Worker." +
                   "Make sure you've registered assembly containing this type");

            return (IActorEndpoint)factory(path.Serialize());
        }

        public static void Reset()
        {
            factories.Clear();
        }

        public static void Register(ActorType type)
        {
            var isActor  = type.Interface.GetCustomAttribute<ActorAttribute>()  != null;
            var isWorker = type.Interface.GetCustomAttribute<WorkerAttribute>() != null;

            if (isActor && isWorker)
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {type}");

            factories.Add(type, isWorker ? GetWorkerFactory()  : GetActorFactory(type));
        }

        static Func<string, object> GetWorkerFactory()
        {
            var factory = GrainFactory();

            return id => factory.GetGrain<IW>(id);
        }

        static Func<string, object> GetActorFactory(ActorType type)
        {
            var factory = GrainFactory();

            var attribute = type.Interface.GetCustomAttribute<ActorAttribute>()
                            ?? new ActorAttribute();

            switch (attribute.Placement)
            {
                case Placement.Random:
                    return id => factory.GetGrain<IA0>(id);;
                case Placement.PreferLocal:
                    return id => factory.GetGrain<IA1>(id);;
                case Placement.DistributeEvenly:
                    return id => factory.GetGrain<IA2>(id);;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static IGrainFactory GrainFactory()
        {
            return (IGrainFactory) Activator.CreateInstance(typeof(GrainFactory), nonPublic: true);
        }
    }
}