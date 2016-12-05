using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Core
{
    using Utility;

    class ActorConfiguration
    {
        public static ActorConfiguration[] From(IEnumerable<Assembly> assemblies) => 
            assemblies.SelectMany(x => x.ActorTypes().Select(Build)).ToArray();

        static ActorConfiguration Build(Type actor)
        {
            var isSingleton = IsSingleton(actor);
            var isWorker = IsWorker(actor);

            if (isSingleton && isWorker)
                throw new InvalidOperationException(
                    $"A type cannot be configured to be both Actor and Worker: {actor}");

            var config = new ActorConfiguration(ActorTypeName.Of(actor), actor);

            SetPlacement(actor, config);
            SetReentrancy(actor, config);
            SetKeepAliveTimeout(actor, config);
            SetAutorun(actor, config);
            SetStickiness(actor, config);
            SetInvoker(actor, config);
            SetWorker(actor, config);

            return config;
        }

        static bool IsWorker(MemberInfo x) => x.GetCustomAttribute<WorkerAttribute>() != null;
        static bool IsSingleton(MemberInfo x) => !IsWorker(x);

        static void SetPlacement(Type actor, ActorConfiguration config)
        {
            var attribute = actor.GetCustomAttribute<ActorAttribute>() ?? new ActorAttribute();
            config.Placement = attribute.Placement;
        }

        static void SetKeepAliveTimeout(Type actor, ActorConfiguration config)
        {
            var timeout = KeepAliveAttribute.Timeout(actor);
            if (timeout != TimeSpan.Zero)
                config.KeepAliveTimeout = timeout;
        }

        static void SetReentrancy(Type actor, ActorConfiguration config)
        {
            bool reentrant;
            config.InterleavePredicate = ReentrantAttribute.Predicate(actor, out reentrant);
            config.Reentrant = reentrant;
        }

        static void SetAutorun(Type actor, ActorConfiguration config)
        {
            var ids = AutorunAttribute.From(actor);
            if (ids.Length > 0)
                config.Autorun(ids);
        }

        static void SetStickiness(Type actor, ActorConfiguration config)
        {
            config.Sticky = StickyAttribute.IsApplied(actor);
        }

        static void SetInvoker(Type actor, ActorConfiguration config)
        {
            var invoker = InvokerAttribute.From(actor);
            if (invoker != null)
                config.Invoker = invoker;
        }

        static void SetWorker(Type actor, ActorConfiguration config)
        {
            config.Worker = IsWorker(actor);
        }

        Func<object, bool> interleavePredicate;
        string invoker;
        public readonly Type Type;
        readonly HashSet<string> autoruns = new HashSet<string>();
        TimeSpan keepAliveTimeout = TimeSpan.Zero;

        protected ActorConfiguration(string name, Type type)
        {
            this.Type = type;
            Requires.NotNullOrWhitespace(name, nameof(name));
            Name = name;
        }

        public string Name { get;}
        public bool Worker { get; set; }
        public bool Reentrant { get; set; }

        public Func<object, bool> InterleavePredicate
        {
            get { return interleavePredicate; }
            set
            {
                if (Reentrant && value != null)
                    throw new InvalidOperationException(
                        $"'{Name}' actor can be designated either as fully reentrant or " +
                        "as partially reentrant (by specifying interleave predicate)");

                interleavePredicate = value;
            }
        }

        public Placement Placement
        {
            get; set;
        }

        public string Invoker
        {
            get { return invoker; }
            set
            {
                Requires.NotNullOrWhitespace(value, nameof(value));
                invoker = value;
            }
        }

        public TimeSpan KeepAliveTimeout
        {
            get { return keepAliveTimeout; }
            set
            {
                if (value < TimeSpan.FromMinutes(1))
                    throw new ArgumentException(
                        "Minimum activation GC timeout is 1 minute", nameof(value));

                keepAliveTimeout = value;
            }
        }

        public void Autorun(params string[] ids)
        {
            Requires.NotNull(ids, nameof(ids));
            Array.ForEach(ids, x => autoruns.Add(x));
        }

        public string[] Autoruns => autoruns.ToArray();

        public bool Sticky { get; set; }
    }
}