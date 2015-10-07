using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;
using Orleans.Providers;

namespace Orleankka.Core
{
    using Client;
    using Cluster;
    using Utility;

    class StreamProvider : IStreamProviderImpl
    {
        static readonly string[] Separator = {":"};

        static readonly Dictionary<string, List<StreamSubscriptionSpecification>> configuration = 
                    new Dictionary<string, List<StreamSubscriptionSpecification>>();

        internal static void Reset() => configuration.Clear();

        internal static void Register(ActorType type)
        {
            var attributes = type.Implementation.GetCustomAttributes<StreamSubscriptionAttribute>(inherit: true);

            foreach (var attribute in attributes)
            {
                if (string.IsNullOrWhiteSpace(attribute.Source))
                    throw new InvalidOperationException($"StreamSubscription attribute defined on '{type.Implementation}' " +
                                                         "has null or whitespace only value of Source");

                if (string.IsNullOrWhiteSpace(attribute.Target))
                    throw new InvalidOperationException($"StreamSubscription attribute defined on '{type.Implementation}' " +
                                                         "has null or whitespace only value of Target");

                var parts = attribute.Source.Split(Separator, 2, StringSplitOptions.None);
                if (parts.Length != 2)
                    throw new InvalidOperationException($"StreamSubscription attribute defined on '{type.Implementation}' " +
                                                         "has invalid Source specification: " + attribute.Source);
                var provider = parts[0];
                var source = parts[1];

                var specifications = configuration.Find(provider);
                if (specifications == null)
                {
                    specifications = new List<StreamSubscriptionSpecification>();
                    configuration.Add(provider, specifications);
                }

                var specification = new StreamSubscriptionSpecification(source, attribute.Target, type.Implementation);
                specifications.Add(specification);
            }
        }

        internal const string TypeKey = "<-::Type::->";

        readonly ConditionalWeakTable<object, object> streams = 
             new ConditionalWeakTable<object, object>();

        List<StreamSubscriptionSpecification> specifications;
        IStreamProviderImpl provider;
        IActorSystem system;

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;

            system = ClusterActorSystem.Initialized 
                ? ClusterActorSystem.Current 
                : ClientActorSystem.Current;

            specifications = configuration.Find(name)
                ?? new List<StreamSubscriptionSpecification>();

            var type = Type.GetType(config.Properties[TypeKey]);
            config.RemoveProperty(TypeKey);
            Debug.Assert(type != null);

            provider = (IStreamProviderImpl)Activator.CreateInstance(type);
            return provider.Init(name, providerRuntime, config);
        }

        public string Name { get; private set; }
        public bool IsRewindable => provider.IsRewindable;

        public Task Start() => provider.Start();
        public Task Stop() => provider.Stop();

        public IAsyncStream<T> GetStream<T>(Guid unused, string id)
        {
            var stream = provider.GetStream<T>(unused, id);

            return (IAsyncStream<T>) streams.GetValue(stream, _ =>
            {
                var recipients = specifications
                    .Where(x => x.Matches(id))
                    .Select(x => x.Target(system.ActorOf, id))
                    .ToArray();

                Func<T, Task> fan = item => TaskDone.Done;

                if (recipients.Length != 0)
                    fan = item => Task.WhenAll(recipients.Select(x => x.Tell(item)));

                return new Stream<T>(stream, fan);
            });
        }
    }
}