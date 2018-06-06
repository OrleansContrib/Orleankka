using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Orleans.Streams;
using Orleans.Providers.Streams.SimpleMessageStream;

using StreamIdentity = Orleans.Internals.StreamIdentity;

namespace Orleankka.Core.Streams
{
    using Microsoft.Extensions.DependencyInjection;

    using Utility;

    class StreamSubscriptionMatcher : IStreamProvider
    {
        static readonly HashSet<string> actors = new HashSet<string>();

        static readonly Dictionary<string, List<StreamSubscriptionSpecification>> configuration = 
                    new Dictionary<string, List<StreamSubscriptionSpecification>>();

        internal static void Register(string actor, IEnumerable<StreamSubscriptionSpecification> specifications)
        {
            if (actors.Contains(actor))
                return;

            foreach (var each in specifications)
            {
                var registry = configuration.Find(each.Provider);

                if (registry == null)
                {
                    registry = new List<StreamSubscriptionSpecification>();
                    configuration.Add(each.Provider, registry);
                }

                registry.Add(each);
            }

            actors.Add(actor);
        }

        public static StreamSubscriptionMatch[] Match(IActorSystem system, StreamIdentity stream)
        {
            var specifications = configuration.Find(stream.Provider)
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

        readonly ConditionalWeakTable<object, object> streams = 
             new ConditionalWeakTable<object, object>();

        readonly IEnumerable<StreamSubscriptionSpecification> specifications;
        readonly IStreamProvider provider;
        readonly IActorSystem system;

        public StreamSubscriptionMatcher(IServiceProvider services, string name)
        {
            system = services.GetRequiredService<IActorSystem>();

            specifications = configuration.Find(name)
                ?? Enumerable.Empty<StreamSubscriptionSpecification>();

            provider = SimpleMessageStreamProvider.Create(services, name);
            Name = name;
        }

        public string Name { get; }
        public bool IsRewindable => provider.IsRewindable;

        public IAsyncStream<T> GetStream<T>(Guid unused, string id)
        {
            var stream = provider.GetStream<T>(unused, id);

            return (IAsyncStream<T>) streams.GetValue(stream, _ =>
            {
                var recipients = Match(system, id, specifications);

                Func<T, Task> fan = item => Task.CompletedTask;

                if (recipients.Length > 0)
                    fan = item => Task.WhenAll(recipients.Select(x => x.Receive(item)));

                return new Stream<T>(stream, fan);
            });
        }
    }
}