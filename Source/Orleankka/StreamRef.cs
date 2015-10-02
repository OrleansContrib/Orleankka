using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka
{
    [Serializable]
    [DebuggerDisplay("s->{ToString()}")]
    public class StreamRef : IAsyncObservable<object>
    {
        public static StreamRef Deserialize(StreamPath path)
        {            
            return new StreamRef(path, path.Proxy());
        }

        readonly IAsyncStream<object> endpoint;

        protected internal StreamRef(StreamPath path)
        {
            Path = path;
        }

        StreamRef(StreamPath path, IAsyncStream<object> endpoint)
        {
            Path = path;
            this.endpoint = endpoint;
        }

        public StreamPath Path { get; }

        public string Serialize()
        {
            return Path.Serialize();
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

        public bool Equals(StreamRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other)
                    || Path.Equals(other.Path));
        }

        public bool Equals(StreamPath other)
        {
            return Path.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType() && Equals((StreamRef)obj));
        }

        public override int GetHashCode() => Path.GetHashCode();

        public static bool operator ==(StreamRef left, StreamRef right) => Equals(left, right);
        public static bool operator !=(StreamRef left, StreamRef right) => !Equals(left, right);

        public override string ToString() => Path.ToString();

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", Path.Serialize(), typeof(string));
        }

        public StreamRef(SerializationInfo info, StreamingContext context)
        {
            var value = (string)info.GetValue("path", typeof(string));
            Path = StreamPath.Deserialize(value);
            endpoint = Path.Proxy();
        }

        #endregion
    }
}