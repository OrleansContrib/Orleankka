using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using Orleans.Streams;
using Orleans.Runtime;
using Orleans.Runtime.Host;

namespace Orleans.Internals
{
    /// <summary>
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public class StreamPubSubWrapper : IStreamPubSub
    {
        public static void Hook(string[] providers, Func<StreamIdentity, StreamPubSubMatch[]> matcher)
        {
            var runtimeType = typeof(SiloHost).Assembly.GetType("Orleans.Runtime.Providers.SiloProviderRuntime");
            var runtime = runtimeType.GetProperty("Instance").GetValue(null);

            var grainBasedPubSubField = runtimeType
                .GetField("grainBasedPubSub",
                          BindingFlags.Instance | BindingFlags.NonPublic);

            var combinedGrainBasedAndImplicitPubSub = runtimeType
                .GetField("combinedGrainBasedAndImplicitPubSub",
                          BindingFlags.Instance | BindingFlags.NonPublic);

            Debug.Assert(grainBasedPubSubField != null);
            Debug.Assert(combinedGrainBasedAndImplicitPubSub != null);

            var streamPubSub = (IStreamPubSub) grainBasedPubSubField.GetValue(runtime);
            var wrapper = new StreamPubSubWrapper(providers, streamPubSub, matcher);
            combinedGrainBasedAndImplicitPubSub.SetValue(runtime, wrapper);
        }

        readonly Func<StreamIdentity, StreamPubSubMatch[]> matcher;
        readonly string[] providers;
        readonly IStreamPubSub registry;

        StreamPubSubWrapper(string[] providers, IStreamPubSub registry, Func<StreamIdentity, StreamPubSubMatch[]> matcher)
        {
            this.providers = providers;
            this.registry = registry;
            this.matcher = matcher;
        }

        bool ShouldMatch(string provider) => providers.Any(x => x == provider);

        async Task<ISet<PubSubSubscriptionState>> IStreamPubSub.RegisterProducer(StreamId streamId, string streamProvider, IStreamProducerExtension streamProducer)
        {
            var matches = new PubSubSubscriptionState[0];

            if (ShouldMatch(streamProvider))
            {
                matches = (from StreamPubSubMatch m in matcher(new StreamIdentity(streamId))
                           let subId = GuidId.GetNewGuidId()
                           select new PubSubSubscriptionState(subId, streamId, new PushExtension(m)))
                          .ToArray();
            }

            var registered = await registry.RegisterProducer(streamId, streamProvider, streamProducer);
            registered.UnionWith(matches);

            return registered;
        }

        Task IStreamPubSub.UnregisterProducer(StreamId streamId, string streamProvider, IStreamProducerExtension streamProducer)
        {
            return registry.UnregisterProducer(streamId, streamProvider, streamProducer);
        }

        Task IStreamPubSub.RegisterConsumer(GuidId subscriptionId, StreamId streamId, string streamProvider, IStreamConsumerExtension streamConsumer, IStreamFilterPredicateWrapper filter)
        {
            return registry.RegisterConsumer(subscriptionId, streamId, streamProvider, streamConsumer, filter);
        }

        Task IStreamPubSub.UnregisterConsumer(GuidId subscriptionId, StreamId streamId, string streamProvider)
        {
            return registry.UnregisterConsumer(subscriptionId, streamId, streamProvider);
        }

        Task<int> IStreamPubSub.ProducerCount(Guid streamId, string streamProvider, string streamNamespace)
        {
            return registry.ProducerCount(streamId, streamProvider, streamNamespace);
        }

        Task<int> IStreamPubSub.ConsumerCount(Guid streamId, string streamProvider, string streamNamespace)
        {
            return registry.ConsumerCount(streamId, streamProvider, streamNamespace);
        }

        Task<List<GuidId>> IStreamPubSub.GetAllSubscriptions(StreamId streamId, IStreamConsumerExtension streamConsumer)
        {
            return registry.GetAllSubscriptions(streamId, streamConsumer);
        }

        GuidId IStreamPubSub.CreateSubscriptionId(StreamId streamId, IStreamConsumerExtension streamConsumer)
        {
            return registry.CreateSubscriptionId(streamId, streamConsumer);
        }

        Task<bool> IStreamPubSub.FaultSubscription(StreamId streamId, GuidId subscriptionId)
        {
            return registry.FaultSubscription(streamId, subscriptionId);
        }
    }
}