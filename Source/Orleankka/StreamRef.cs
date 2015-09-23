using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka
{
    public class StreamRef : IAsyncObservable<object>
    {
        public static StreamRef Deserialize(StreamPath path)
        {            
            return new StreamRef(path, path.Proxy());
        }

        readonly StreamPath path;
        readonly IAsyncStream<object> endpoint;

        StreamRef(StreamPath path, IAsyncStream<object> endpoint)
        {
            this.path = path;
            this.endpoint = endpoint;
        }

        public StreamPath Path
        {
            get { return path; }
        }

        public string Namespace
        {
            get { return endpoint.Namespace; }
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

        public virtual Task<StreamSubscriptionHandle<object>> SubscribeAsync(IAsyncObserver<object> observer)
        {
            return endpoint.SubscribeAsync(observer);
        }

        public virtual Task<StreamSubscriptionHandle<object>> SubscribeAsync(IAsyncObserver<object> observer, StreamSequenceToken token, StreamFilterPredicate filterFunc = null, object filterData = null)
        {
            return endpoint.SubscribeAsync(observer, token, filterFunc, filterData);
        }

        public Task<IList<StreamSubscriptionHandle<object>>> GetAllSubscriptionHandles()
        {
            return endpoint.GetAllSubscriptionHandles();
        }
    }
}