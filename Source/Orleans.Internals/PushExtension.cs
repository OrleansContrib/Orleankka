using System;
using System.Threading.Tasks;

using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Concurrency;

namespace Orleans.Internals
{
    [Serializable]
    class PushExtension : GrainReference, IStreamConsumerExtension
    {
        static readonly GrainReference Mock = FromGrainId(GrainId.NewId());

        readonly Func<object, Task> handler;

        public PushExtension(StreamPubSubMatch match) 
            : base(Mock)
        {
            handler = match.Handler;
        }

        public async Task<StreamSequenceToken> DeliverBatch(GuidId subscriptionId, Immutable<IBatchContainer> batch, StreamSequenceToken prevToken)
        {
            foreach (var each in batch.Value.GetEvents<object>())
                await handler(each.Item1);

            return null;
        }

        public async Task<StreamSequenceToken> DeliverItem(GuidId subscriptionId, Immutable<object> item, StreamSequenceToken currentToken, StreamSequenceToken prevToken)
        {
            await handler(item);
            return null;
        }

        public Task<StreamSequenceToken> GetSequenceToken(GuidId subscriptionId) 
            => Task.FromResult((StreamSequenceToken)null);

        public Task CompleteStream(GuidId subscriptionId) => TaskDone.Done;
        public Task ErrorInStream(GuidId subscriptionId, Exception exc) => TaskDone.Done;
    }
}