using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Streams;
using Orleans.Providers.Streams.SimpleMessageStream;

namespace Orleankka.Legacy.Streams
{
    class StreamSubscriptionMatcher : IStreamProvider
    {
        internal static StreamSubscriptionMatch[] Match(IActorSystem system, string stream, IEnumerable<StreamSubscriptionSpecification> specifications) => 
            specifications
            .Select(s => s.Match(system, stream))
            .Where(m => m != StreamSubscriptionMatch.None)
            .ToArray();

        readonly ConditionalWeakTable<object, object> streams = 
             new ConditionalWeakTable<object, object>();

        readonly IEnumerable<StreamSubscriptionSpecification> specifications;
        readonly IStreamProvider provider;
        readonly IActorSystem system;

        public StreamSubscriptionMatcher(IServiceProvider services, string name)
        {
            system = services.GetRequiredService<IActorSystem>();

            var registry = services.GetRequiredService<StreamSubscriptionSpecificationRegistry>();
            specifications = registry.Find(name);

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