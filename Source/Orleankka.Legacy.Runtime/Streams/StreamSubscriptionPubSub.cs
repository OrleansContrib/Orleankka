using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Orleankka.Legacy.Streams
{
    /// <summary>
    ///     StreamPubSub implementation for <see cref="StreamSubscriptionAttribute" />
    /// </summary>
    class StreamSubscriptionPubSub : IStreamPubSub
    {
        readonly StreamSubscriptionTable streamSubscriptionTable;

        public StreamSubscriptionPubSub(StreamSubscriptionTable streamSubscriptionTable)
        {
            this.streamSubscriptionTable = streamSubscriptionTable;
        }

        public Task<ISet<PubSubSubscriptionState>> RegisterProducer(QualifiedStreamId streamId, GrainId streamProducer)
        {
            var implicitStreamSubscriptions = streamSubscriptionTable.GetStreamSubscriptions(streamId);

            ISet<PubSubSubscriptionState> result = new HashSet<PubSubSubscriptionState>();
            foreach (var subscription in implicitStreamSubscriptions)
            {
                var subscriptionId = GuidId.GetGuidId(streamSubscriptionTable.GetSubscriptionId(subscription, streamId));
                var streamConsumer = streamSubscriptionTable.GetStreamSubscriptionMessageDeliverer(subscription);
                result.Add(new PubSubSubscriptionState(subscriptionId, streamId, streamConsumer));
            }

            return Task.FromResult(result);
        }

        public Task UnregisterProducer(QualifiedStreamId streamId, GrainId streamProducer)
        {
            return Task.CompletedTask;
        }

        public Task RegisterConsumer(GuidId subscriptionId, QualifiedStreamId streamId, GrainId streamConsumer, string filterData)
        {
            if (!IsStreamSubscriber(streamConsumer, streamId))
                throw new ArgumentOutOfRangeException(streamId.ToString(), "Only implicit stream subscriptions are supported.");

            return Task.CompletedTask;
        }

        public Task UnregisterConsumer(GuidId subscriptionId, QualifiedStreamId streamId)
        {
            if (!IsStreamSubscriber(subscriptionId, streamId))
                throw new ArgumentOutOfRangeException(streamId.ToString(), "Only implicit stream subscriptions are supported.");

            return Task.CompletedTask;
        }

        public Task<int> ProducerCount(QualifiedStreamId streamId)
        {
            return Task.FromResult(0);
        }

        public Task<int> ConsumerCount(QualifiedStreamId streamId)
        {
            return Task.FromResult(0);
        }

        public Task<List<StreamSubscription>> GetAllSubscriptions(QualifiedStreamId streamId, GrainId streamConsumer = new())
        {
            if (!IsStreamSubscriber(streamConsumer, streamId))
                return Task.FromResult(new List<StreamSubscription>());

            if (streamConsumer != default)
            {
                var subscriptionId = CreateSubscriptionId(streamId, streamConsumer);
                return Task.FromResult(new List<StreamSubscription> { new(subscriptionId.Guid, streamId.ProviderName, streamId, streamConsumer) });
            }

            var implicitStreamSubscriptions = streamSubscriptionTable.GetStreamSubscriptions(streamId);
            var result = new List<StreamSubscription>();
            foreach (var subscription in implicitStreamSubscriptions)
            {
                var subscriptionId = streamSubscriptionTable.GetSubscriptionId(subscription, streamId);
                var consumer = streamSubscriptionTable.GetStreamSubscriptionMessageDeliverer(subscription);
                result.Add(new StreamSubscription(subscriptionId, streamId.ProviderName, streamId.StreamId, consumer));
            }

            return Task.FromResult(result);
        }

        public GuidId CreateSubscriptionId(QualifiedStreamId streamId, GrainId streamConsumer)
        {
            if (!IsStreamSubscriber(streamConsumer, streamId))
                throw new ArgumentOutOfRangeException(streamId.ToString(), "Only implicit stream subscriptions are supported.");

            return GuidId.GetGuidId(streamSubscriptionTable.GetSubscriptionId(streamConsumer, streamId));
        }

        public Task<bool> FaultSubscription(QualifiedStreamId streamId, GuidId subscriptionId)
        {
            return Task.FromResult(false);
        }

        internal bool IsStreamSubscriber(GuidId subscriptionId, QualifiedStreamId streamId)
        {
            return streamSubscriptionTable.IsStreamSubscriptionSubscriber(subscriptionId, streamId);
        }

        internal bool IsStreamSubscriber(GrainId streamConsumer, QualifiedStreamId streamId)
        {
            var implicitStreamSubscriptions = streamSubscriptionTable.GetStreamSubscriptions(streamId);
            if (implicitStreamSubscriptions.Any(subscription => streamSubscriptionTable.GetStreamSubscriptionMessageDeliverer(subscription) == streamConsumer))
                return true;

            return false;
        }
    }
}