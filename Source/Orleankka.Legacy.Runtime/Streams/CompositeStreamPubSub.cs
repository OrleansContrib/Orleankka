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
    ///     StreamPubSub that combines orleans `StreamPubSubImpl` and <see cref="StreamSubscriptionPubSub" />.
    /// </summary>
    class CompositeStreamPubSub : IStreamPubSub
    {
        readonly IStreamPubSub orleansPubSub;
        readonly StreamSubscriptionPubSub streamSubscriptionPubSub;

        public CompositeStreamPubSub(IStreamPubSub orleansPubSub, StreamSubscriptionPubSub streamSubscriptionPubSub)
        {
            this.orleansPubSub = orleansPubSub;
            this.streamSubscriptionPubSub = streamSubscriptionPubSub;
        }

        public async Task<ISet<PubSubSubscriptionState>> RegisterProducer(QualifiedStreamId streamId, GrainId streamProducer)
        {
            var orleansRes = await orleansPubSub.RegisterProducer(streamId, streamProducer);
            var streamSubscriptionRes = await streamSubscriptionPubSub.RegisterProducer(streamId, streamProducer);
            orleansRes.UnionWith(streamSubscriptionRes);
            return orleansRes;
        }

        public Task UnregisterProducer(QualifiedStreamId streamId, GrainId streamProducer)
        {
            return orleansPubSub.UnregisterProducer(streamId, streamProducer);
        }

        public Task RegisterConsumer(GuidId subscriptionId, QualifiedStreamId streamId, GrainId streamConsumer, string filterData)
        {
            return streamSubscriptionPubSub.IsStreamSubscriber(streamConsumer, streamId)
            ? streamSubscriptionPubSub.RegisterConsumer(subscriptionId, streamId, streamConsumer, filterData)
            : orleansPubSub.RegisterConsumer(subscriptionId, streamId, streamConsumer, filterData);
        }

        public Task UnregisterConsumer(GuidId subscriptionId, QualifiedStreamId streamId)
        {
            return streamSubscriptionPubSub.IsStreamSubscriber(subscriptionId, streamId)
            ? streamSubscriptionPubSub.UnregisterConsumer(subscriptionId, streamId)
            : orleansPubSub.UnregisterConsumer(subscriptionId, streamId);
        }

        public Task<int> ProducerCount(QualifiedStreamId streamId)
        {
            return orleansPubSub.ProducerCount(streamId);
        }

        public Task<int> ConsumerCount(QualifiedStreamId streamId)
        {
            return orleansPubSub.ConsumerCount(streamId);
        }

        public async Task<List<StreamSubscription>> GetAllSubscriptions(QualifiedStreamId streamId, GrainId streamConsumer)
        {
            if (streamConsumer != default)
            {
                return streamSubscriptionPubSub.IsStreamSubscriber(streamConsumer, streamId)
                ? await streamSubscriptionPubSub.GetAllSubscriptions(streamId, streamConsumer)
                : await orleansPubSub.GetAllSubscriptions(streamId, streamConsumer);
            }

            var streamSubscriptionSubs = await streamSubscriptionPubSub.GetAllSubscriptions(streamId);
            var orleansSubs = await orleansPubSub.GetAllSubscriptions(streamId);
            return streamSubscriptionSubs.Concat(orleansSubs).ToList();
        }

        public GuidId CreateSubscriptionId(QualifiedStreamId streamId, GrainId streamConsumer)
        {
            return streamSubscriptionPubSub.IsStreamSubscriber(streamConsumer, streamId)
            ? streamSubscriptionPubSub.CreateSubscriptionId(streamId, streamConsumer)
            : orleansPubSub.CreateSubscriptionId(streamId, streamConsumer);
        }

        public Task<bool> FaultSubscription(QualifiedStreamId streamId, GuidId subscriptionId)
        {
            return streamSubscriptionPubSub.IsStreamSubscriber(subscriptionId, streamId)
            ? streamSubscriptionPubSub.FaultSubscription(streamId, subscriptionId)
            : orleansPubSub.FaultSubscription(streamId, subscriptionId);
        }
    }
}