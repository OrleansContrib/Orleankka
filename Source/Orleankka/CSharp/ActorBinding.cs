using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.CSharp
{
    using Core;

    class ActorBinding
    {
        internal static IActorActivator Activator;

        internal static void Reset() => 
            Activator = new DefaultActorActivator();

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

        static void SetStreamSubscriptions(Type actor, ActorConfiguration config)
        {
            var subscriptions = StreamSubscriptionBinding.From(actor, new Dispatcher(actor));
            config.Subscriptions.AddRange(subscriptions);
        }

        static void SetReceiver(Type actor, ActorConfiguration config)
        {
            var dispatcher = new Dispatcher(actor);

            config.Receiver = (id, context) =>
            {
                var runtime = new ActorRuntime(context);

                var instance = Activator.Activate(actor, id, runtime);
                instance.Initialize(id, runtime, dispatcher);

                return async (_, message) =>
                {
                    if (message is Activate)
                    {
                        await instance.OnActivate();
                        return null;
                    }

                    if (message is Deactivate)
                    {
                        await instance.OnDeactivate();
                        return null;
                    }

                    if (message is Reminder)
                    {
                        await instance.OnReminder(((Reminder) message).Name);
                        return null;
                    }

                    return await instance.OnReceive(message);
                };
            };
        }
    }
}
