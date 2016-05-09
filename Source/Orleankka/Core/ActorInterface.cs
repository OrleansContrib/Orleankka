using System;
using System.Reflection;

using Orleans;

namespace Orleankka.Core
{
    using Endpoints;

    class ActorInterface
    {
        internal static ActorInterface From(Type type)
        {
            var isActor = type.GetCustomAttribute<ActorAttribute>() != null;
            var isWorker = type.GetCustomAttribute<WorkerAttribute>() != null;

            if (isActor && isWorker)
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {type}");

            var factory = isWorker 
                ? GetWorkerFactory() 
                : GetActorFactory(type);

            return new ActorInterface(type, factory);
        }

        static Func<string, object> GetWorkerFactory()
        {
            var factory = GrainFactory();
            return id => factory.GetGrain<IW>(id);
        }

        static Func<string, object> GetActorFactory(Type type)
        {
            var factory = GrainFactory();

            var attribute = type.GetCustomAttribute<ActorAttribute>()
                            ?? new ActorAttribute();

            switch (attribute.Placement)
            {
                case Placement.Random:
                    return id => factory.GetGrain<IA0>(id); ;
                case Placement.PreferLocal:
                    return id => factory.GetGrain<IA1>(id); ;
                case Placement.DistributeEvenly:
                    return id => factory.GetGrain<IA2>(id); ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static IGrainFactory GrainFactory()
        {
            return (IGrainFactory)Activator.CreateInstance(typeof(GrainFactory), nonPublic: true);
        }

        readonly Reentrant reentrant;
        readonly Func<string, object> factory;

        ActorInterface(Type type, Func<string, object> factory)
        {
            this.factory = factory;
            reentrant = new Reentrant(type);
            Type = type;
        }

        internal Type Type { get; private set; }
        internal bool IsReentrant(object message) => reentrant.IsReentrant(message);
        internal IActorEndpoint Proxy(ActorPath path) => (IActorEndpoint)factory(path.Serialize());        
    }
}