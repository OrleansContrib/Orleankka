using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Orleankka.Client;
using Orleankka.Cluster;
using Orleankka.Utility;

using Orleans;
using Orleans.Internals;
using Orleans.Providers;
using Orleans.Streams;

namespace Orleankka.Core.Streams
{
    class StreamSubscriptionMatcher : IStreamProviderImpl
    {
        static readonly Dictionary<string, List<StreamSubscriptionSpecification>> configuration = 
                    new Dictionary<string, List<StreamSubscriptionSpecification>>();

        internal static void Reset() => configuration.Clear();

        internal static void Register(ActorType type)
        {
            foreach (var specification in StreamSubscriptionSpecification.From(type))
            {
                var specifications = DictionaryExtensions.Find(configuration, specification.Provider);

                if (specifications == null)
                {
                    specifications = new List<StreamSubscriptionSpecification>();
                    configuration.Add(specification.Provider, specifications);
                }

                specifications.Add(specification);
            }
        }

        public static StreamSubscriptionMatch[] Match(IActorSystem system, StreamIdentity stream)
        {
            var specifications = DictionaryExtensions.Find(configuration, stream.Provider)
                ?? Enumerable.Empty<StreamSubscriptionSpecification>();

            return Match(system, stream.Id, specifications);
        }

        static StreamSubscriptionMatch[] Match(IActorSystem system, string stream, IEnumerable<StreamSubscriptionSpecification> specifications)
        {
            return specifications
                    .Select(s => s.Match(system, stream))
                    .Where(m => m != StreamSubscriptionMatch.None)
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

            specifications = DictionaryExtensions.Find(configuration, name)
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
        public Task Close() => provider.Close();

        public IAsyncStream<T> GetStream<T>(Guid unused, string id)
        {
            var stream = provider.GetStream<T>(unused, id);

            return (IAsyncStream<T>) streams.GetValue(stream, _ =>
            {
                var recipients = Match(system, id, specifications);

                Func<T, Task> fan = item => TaskDone.Done;

                if (recipients.Length > 0)
                    fan = item => Task.WhenAll(recipients.Select(x => x.Receive(item)));

                return new Stream<T>(stream, fan);
            });
        }
    }
}