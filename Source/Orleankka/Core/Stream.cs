using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka.Core
{
    class Stream<T> : IAsyncStream<T>
    {
        readonly IAsyncStream<T> stream;
        readonly Func<T, Task> fan;

        public Stream(IAsyncStream<T> stream, Func<T, Task> fan)
        {
            this.stream = stream;
            this.fan = fan;
        }

        public Task OnNextAsync(T item, StreamSequenceToken token = null)
        {
            return Task.WhenAll(stream.OnNextAsync(item, token), fan(item));
        }

        #region Uninteresting Delegation (Nothing To See Here)

        public Guid Guid => stream.Guid;
        public string Namespace => stream.Namespace;
        public bool Equals(IAsyncStream<T> other) => stream.Equals(other);
        public int CompareTo(IAsyncStream<T> other) => stream.CompareTo(other);
        public Task<StreamSubscriptionHandle<T>> SubscribeAsync(IAsyncObserver<T> observer) => stream.SubscribeAsync(observer);

        public Task<StreamSubscriptionHandle<T>> SubscribeAsync(
            IAsyncObserver<T> observer,
            StreamSequenceToken token,
            StreamFilterPredicate filterFunc = null,
            object filterData = null) => stream.SubscribeAsync(observer, token, filterFunc, filterData);

        public Task OnCompletedAsync() => stream.OnCompletedAsync();
        public Task OnErrorAsync(Exception ex) => stream.OnErrorAsync(ex);
        public Task OnNextBatchAsync(IEnumerable<T> batch, StreamSequenceToken token = null) => stream.OnNextBatchAsync(batch, token);
        public Task<IList<StreamSubscriptionHandle<T>>> GetAllSubscriptionHandles() => stream.GetAllSubscriptionHandles();
        public bool IsRewindable => stream.IsRewindable;
        public string ProviderName => stream.ProviderName;

        #endregion
    }
}