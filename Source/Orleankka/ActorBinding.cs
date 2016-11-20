using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleankka.Utility;

namespace Orleankka
{
    class ActorBinding
    {
        internal static string[] Conventions;

        internal static Dispatcher Dispatcher(Type actor) => 
            dispatchers.Find(actor) ?? new Dispatcher(actor, Conventions);

        static readonly Dictionary<Type, Dispatcher> dispatchers = 
                    new Dictionary<Type, Dispatcher>(); 

        internal static IActorActivator Activator;

        internal static void Reset()
        {
            ActorTypeName.Reset();
            Activator = new DefaultActorActivator();
            dispatchers.Clear();
            Conventions = null;
        }

        static ActorBinding()
        {
            Reset();
        }

        public static EndpointConfiguration[] Bind(Assembly[] assemblies)
        {
            return assemblies.SelectMany(ScanImplementations).Concat(
                   assemblies.SelectMany(ScanInterfaces).Where(x => x != null)).ToArray();
        }

        static IEnumerable<EndpointConfiguration> ScanImplementations(Assembly assembly) => assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Actor).IsAssignableFrom(type))
            .Select(BuildImplementation);

        static IEnumerable<EndpointConfiguration> ScanInterfaces(Assembly assembly) => assembly.GetTypes()
            .Where(type => type != typeof(IActor) && type.IsInterface && typeof(IActor).IsAssignableFrom(type))
            .Select(BuildInterface);

        static EndpointConfiguration BuildImplementation(Type actor)
        {
            var isActor  = IsActor(actor);
            var isWorker = IsWorker(actor);

            if (isActor && isWorker)
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {actor}");

            return isActor ? BuildActor(actor) : BuildWorker(actor);
        }

        static EndpointConfiguration BuildInterface(Type actor)
        {
            if (ActorTypeName.IsRegistered(actor))
                return null; // assume endpoint was already build from implementation

            var config = new ActorConfiguration(ActorTypeName.Register(actor));
            SetReentrancy(actor, config);

            return config;
        }

        static EndpointConfiguration BuildActor(Type actor)
        {
            var config = new ActorConfiguration(ActorTypeName.Register(actor));

            SetPlacement(actor, config);
            SetReentrancy(actor, config);
            SetKeepAliveTimeout(actor, config);
            SetReceiver(actor, config);
            SetStreamSubscriptions(actor, config);
            SetAutorun(actor, config);
            SetStickiness(actor, config);

            return config;
        }

        static EndpointConfiguration BuildWorker(Type worker)
        {
            var config = new WorkerConfiguration(ActorTypeName.Register(worker));

            SetReentrancy(worker, config);
            SetKeepAliveTimeout(worker, config);
            SetReceiver(worker, config);
            SetStreamSubscriptions(worker, config);
            SetAutorun(worker, config);
            SetStickiness(worker, config);

            return config;
        }

        static bool IsWorker(MemberInfo x) => x.GetCustomAttribute<WorkerAttribute>() != null;
        static bool IsActor(MemberInfo x)  => !IsWorker(x);

        static void SetPlacement(Type actor, ActorConfiguration config)
        {
            var attribute = actor.GetCustomAttribute<ActorAttribute>() ?? new ActorAttribute();
            config.Placement = attribute.Placement;
        }

        static void SetKeepAliveTimeout(Type actor, EndpointConfiguration config)
        {
            var timeout = KeepAliveAttribute.Timeout(actor);
            if (timeout != TimeSpan.Zero)
                config.KeepAliveTimeout = timeout;
        }

        static void SetReentrancy(Type actor, EndpointConfiguration config)
        {
            bool reentrant;
            config.InterleavePredicate = ReentrantAttribute.Predicate(actor, out reentrant);
            config.Reentrant = reentrant;
        }

        static void SetReceiver(Type actor, EndpointConfiguration config)
        {
            dispatchers.Add(actor, new Dispatcher(actor));

            config.Activator = (path, runtime) =>
            {
                var dispatcher = dispatchers[actor];

                var instance = Activator.Activate(actor, path.Id, runtime, dispatcher);
                instance.Initialize(path, runtime, dispatcher);

                return instance;
            };
        }

        static void SetStreamSubscriptions(Type actor, EndpointConfiguration config)
        {
            var subscriptions = StreamSubscriptionBinding.From(actor, dispatchers[actor]);
            foreach (var subscription in subscriptions)
                config.Add(subscription);
        }

        static void SetAutorun(Type actor, EndpointConfiguration config)
        {
            var ids = AutorunAttribute.From(actor);
            if (ids.Length > 0)
                config.Autorun(ids);
        }

        static void SetStickiness(Type actor, EndpointConfiguration config)
        {
            config.Sticky = StickyAttribute.IsApplied(actor);
        }
    }
}
