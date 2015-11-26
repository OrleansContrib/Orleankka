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

        public async Task<StreamHandshakeToken> DeliverBatch(GuidId subscriptionId, Immutable<IBatchContainer> batch, StreamHandshakeToken handshakeToken)
        {
            foreach (var each in batch.Value.GetEvents<object>())
                await handler(each.Item1);

            return null;
        }

        public async Task<StreamHandshakeToken> DeliverItem(GuidId subscriptionId, Immutable<object> item, StreamSequenceToken currentToken, StreamHandshakeToken handshakeToken)
        {
            await handler(item.Value);
            return null;
        }

        public Task<StreamHandshakeToken> GetSequenceToken(GuidId subscriptionId) => Task.FromResult((StreamHandshakeToken)null);
        public Task CompleteStream(GuidId subscriptionId) => TaskDone.Done;
        public Task ErrorInStream(GuidId subscriptionId, Exception exc) => TaskDone.Done;
    }
}