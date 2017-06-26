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
        readonly Func<object, Task> handler;

        public PushExtension(IRuntimeClient client, StreamPubSubMatch match) 
            : base(FromGrainId(GrainId.NewId(), client))
        {
            handler = match.Handler;
        }

        public async Task<StreamHandshakeToken> DeliverImmutable(GuidId subscriptionId, StreamId streamId, Immutable<object> item, StreamSequenceToken currentToken, StreamHandshakeToken handshakeToken)
        {
            await handler(item);

            return null;
        }

        public async Task<StreamHandshakeToken> DeliverMutable(GuidId subscriptionId, StreamId streamId, object item, StreamSequenceToken currentToken, StreamHandshakeToken handshakeToken)
        {
            await handler(item);

            return null;
        }

        public async Task<StreamHandshakeToken> DeliverBatch(GuidId subscriptionId, StreamId streamId, Immutable<IBatchContainer> item, StreamHandshakeToken handshakeToken)
        {
            foreach (var each in item.Value.GetEvents<object>())
                await handler(each.Item1);

            return null;
        }

        public Task<StreamHandshakeToken> GetSequenceToken(GuidId subscriptionId) => Task.FromResult((StreamHandshakeToken)null);

        public Task CompleteStream(GuidId subscriptionId) => TaskDone.Done;
        public Task ErrorInStream(GuidId subscriptionId, Exception exc) => TaskDone.Done;
    }
}