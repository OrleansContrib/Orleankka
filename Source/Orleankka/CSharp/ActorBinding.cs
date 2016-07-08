using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.CSharp
{
    class ActorBinding
    {
        static readonly Dictionary<Type, Dispatcher> dispatchers = 
                    new Dictionary<Type, Dispatcher>(); 

        internal static IActorActivator Activator;

        internal static void Reset()
        {
            Activator = new DefaultActorActivator();
            dispatchers.Clear();
        }

        static ActorBinding()
        {
            Reset();
        }

        public static EndpointConfiguration[] Bind(IEnumerable<Assembly> assemblies) => 
            assemblies.SelectMany(Scan).ToArray();

        static IEnumerable<EndpointConfiguration> Scan(Assembly assembly) => assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Actor).IsAssignableFrom(type))
            .Select(Build);

        static EndpointConfiguration Build(Type actor)
        {
            var isActor  = IsActor(actor);
            var isWorker = IsWorker(actor);

            if (isActor && isWorker)
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {actor}");

            return isActor ? BuildActor(actor) : BuildWorker(actor);
        }

        static EndpointConfiguration BuildActor(Type actor)
        {
            var config = new ActorConfiguration(ActorTypeCode.Of(actor));

            SetPlacement(actor, config);
            SetReentrancy(actor, config);
            SetKeepAliveTimeout(actor, config);
            SetReceiver(actor, config);
            SetStreamSubscriptions(actor, config);

            return config;
        }

        static EndpointConfiguration BuildWorker(Type worker)
        {
            var config = new WorkerConfiguration(ActorTypeCode.Of(worker));

            SetReentrancy(worker, config);
            SetKeepAliveTimeout(worker, config);
            SetReceiver(worker, config);
            SetStreamSubscriptions(worker, config);

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
            config.Reentrancy = ReentrantAttribute.Predicate(actor);
        }

        static void SetReceiver(Type actor, EndpointConfiguration config)
        {
            dispatchers.Add(actor, new Dispatcher(actor));

            config.Receiver = (id, context) =>
            {
                var dispatcher = dispatchers[actor];

                var instance = Activator.Activate(actor, context, dispatcher);
                instance.Initialize(context, dispatcher);

                return async (_, message) => await instance.OnReceive(message);
            };
        }

        static void SetStreamSubscriptions(Type actor, EndpointConfiguration config)
        {
            var subscriptions = StreamSubscriptionBinding.From(actor, dispatchers[actor]);
            foreach (var subscription in subscriptions)
                config.Add(subscription);
        }
    }
}
