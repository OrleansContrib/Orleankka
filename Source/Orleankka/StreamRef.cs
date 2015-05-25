using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

namespace Orleankka
{
    public class StreamRef
    {
        public static StreamRef Deserialize(StreamPath path)
        {            
            return new StreamRef(path.Proxy());
        }

        readonly IAsyncStream<object> endpoint;

        StreamRef(IAsyncStream<object> endpoint)
        {
            this.endpoint = endpoint;
        }

        public virtual Task<StreamSubscriptionHandle<object>> SubscribeAsync(IAsyncObserver<object> observer)
        {
            return endpoint.SubscribeAsync(observer);
        }

        public virtual Task OnNextAsync(object item, StreamSequenceToken token = null)
        {
            return endpoint.OnNextAsync(item, token);
        }

        public virtual Task OnNextBatchAsync(IEnumerable<object> batch, StreamSequenceToken token = null)
        {
            return endpoint.OnNextBatchAsync(batch, token);
        }

        public virtual Task OnCompletedAsync()
        {
            return endpoint.OnCompletedAsync();
        }

        public virtual Task OnErrorAsync(Exception ex)
        {
            return endpoint.OnErrorAsync(ex);
        }
    }
}