using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Placement;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Orleankka.Legacy.Streams
{
    interface IStreamSubscriptionMessageDeliverer : IGrainWithStringKey
    { }

    [PreferLocalPlacement]
    class StreamSubscriptionMessageDeliverer : Grain, IStreamSubscriptionMessageDeliverer, IStreamSubscriptionObserver, IAsyncObserver<object>
    {
        readonly StreamSubscriptionTable table;

        public StreamSubscriptionMessageDeliverer(StreamSubscriptionTable table)
        {
            this.table = table;
        }

        StreamSubscriptionMatch SubscriptionMatch { get; set; }
        QualifiedStreamId StreamId { get; set; }
        Guid SubscriptionId { get; set; }

        public Task OnNextAsync(object item, StreamSequenceToken token = null)
        {
            if (!SubscriptionMatch.ShouldSendMessage(item))
                return Task.CompletedTask;

            return OnNextAsyncSend(item, token);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
        {
            var qualifiedStreamId = new QualifiedStreamId(handleFactory.ProviderName, handleFactory.StreamId);
            var subscriptionMatch = table.GetStreamSubscription(qualifiedStreamId, handleFactory.SubscriptionId.Guid);

            SubscriptionMatch = subscriptionMatch;
            StreamId = qualifiedStreamId;
            SubscriptionId = handleFactory.SubscriptionId.Guid;

            var handle = handleFactory.Create<object>();
            await handle.ResumeAsync(this);
        }

        async Task OnNextAsyncSend(object item, StreamSequenceToken token = null)
        {
            var target = SubscriptionMatch.SelectTarget(item);
            var actor = GrainFactory.GetGrain(SubscriptionMatch.InterfaceType, target).AsReference<IActorGrain>();
            RequestContext.Set(nameof(StreamSequenceToken), token);
            await actor.ReceiveTell(item);
        }
    }
}