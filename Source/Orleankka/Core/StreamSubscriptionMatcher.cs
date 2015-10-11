using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;
using Orleans.Providers;
using Orleans.Internals;

namespace Orleankka.Core
{
    using Client;
    using Cluster;
    using Utility;

    class StreamSubscriptionMatcher : IStreamProviderImpl
    {
        static readonly Dictionary<string, List<StreamSubscriptionSpecification>> configuration = 
                    new Dictionary<string, List<StreamSubscriptionSpecification>>();

        internal static void Reset() => configuration.Clear();

        internal static void Register(ActorType type)
        {
            foreach (var specification in StreamSubscriptionSpecification.From(type))
            {
                var specifications = configuration.Find(specification.Provider);

                if (specifications == null)
                {
                    specifications = new List<StreamSubscriptionSpecification>();
                    configuration.Add(specification.Provider, specifications);
                }

                specifications.Add(specification);
            }
        }

        public static ActorRef[] Match(IActorSystem system, StreamIdentity stream)
        {
            var specifications = configuration.Find(stream.Provider)
                ?? Enumerable.Empty<StreamSubscriptionSpecification>();

            return Match(system, stream.Id, specifications);
        }

        static ActorRef[] Match(IActorSystem system, string stream, IEnumerable<StreamSubscriptionSpecification> specifications)
        {
            return specifications
                    .Select(s => s.Match(stream))
                    .Where(m => !m.Equals(StreamSubscriptionMatch.None))
                    .Select(m => system.ActorOf(m.Actor, m.Id))
                    .ToArray();
        }

        internal const string TypeKey = "<-::Type::->";

        readonly ConditionalWeakTable<object, object> streams = 
             new ConditionalWeakTable<object, object>();

        IEnumerable<StreamSubscriptionSpecification> specifications;
        IStreamProviderImpl provider;
        IActorSystem system;

        public Task Init(string name, IProviderRuntime pr, IProviderConfiguration pc)
        {
            Name = name;

            system = ClusterActorSystem.Initialized 
                ? ClusterActorSystem.Current 
                : ClientActorSystem.Current;

            specifications = configuration.Find(name)
                ?? Enumerable.Empty<StreamSubscriptionSpecification>();

            var type = Type.GetType(pc.Properties[TypeKey]);
            pc.RemoveProperty(TypeKey);
            Debug.Assert(type != null);

            provider = (IStreamProviderImpl)Activator.CreateInstance(type);
            return provider.Init(name, pr, pc);
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
                var recipients = Match(system, id, specifications);

                Func<T, Task> fan = item => TaskDone.Done;

                if (recipients.Length > 0)
                    fan = item => Task.WhenAll(recipients.Select(x => x.Tell(item)));

                return new Stream<T>(stream, fan);
            });
        }
    }
}