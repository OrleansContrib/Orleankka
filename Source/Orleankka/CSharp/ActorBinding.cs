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

        public static ActorConfiguration[] Bind(IEnumerable<Assembly> assemblies) => 
            assemblies.SelectMany(Scan).ToArray();

        static IEnumerable<ActorConfiguration> Scan(Assembly assembly) => assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Actor).IsAssignableFrom(type))
            .Select(Build);

        static ActorConfiguration Build(Type actor)
        {
            var config = new ActorConfiguration(ActorTypeCode.Of(actor));
           
            SetActorKind(actor, config);
            SetPlacement(actor, config);
            SetReentrancy(actor, config);
            SetKeepAliveTimeout(actor, config);
            SetReceiver(actor, config);
            SetStreamSubscriptions(actor, config);

            return config;
        }

        static void SetActorKind(Type actor, ActorConfiguration config)
        {
            var isActor = actor.GetCustomAttribute<ActorAttribute>() != null;
            var isWorker = actor.GetCustomAttribute<WorkerAttribute>() != null;

            if (isActor && isWorker)
            {
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {actor}");
            }

            config.Worker = isWorker;
        }

        static void SetPlacement(Type actor, ActorConfiguration config)
        {
            if (!config.Worker)
            {
                var attribute = actor.GetCustomAttribute<ActorAttribute>() ?? new ActorAttribute();
                config.Placement = attribute.Placement;
            }
        }

        static void SetKeepAliveTimeout(Type actor, ActorConfiguration config)
        {
            var timeout = KeepAliveAttribute.Timeout(actor);
            if (timeout != TimeSpan.Zero)
                config.KeepAliveTimeout = timeout;
        }

        static void SetReentrancy(Type actor, ActorConfiguration config)
        {
            config.Reentrancy = ReentrantAttribute.Predicate(actor);
        }

        static void SetReceiver(Type actor, ActorConfiguration config)
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

        static void SetStreamSubscriptions(Type actor, ActorConfiguration config)
        {
            var subscriptions = StreamSubscriptionBinding.From(actor, dispatchers[actor]);
            foreach (var subscription in subscriptions)
                config.Add(subscription);
        }
    }
}
