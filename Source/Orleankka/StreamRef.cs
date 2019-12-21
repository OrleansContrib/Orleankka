using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleans.CodeGeneration;
using Orleans.Concurrency;
using Orleans.Serialization;
using Orleans.Streams;
using Orleans.Runtime;

namespace Orleankka
{
    using Utility;

    [Serializable, Immutable]
    [DebuggerDisplay("s->{ToString()}")]
    public class StreamRef : IEquatable<StreamRef>, IEquatable<StreamPath>
    {
        [NonSerialized]
        readonly IStreamProvider provider;

        protected internal StreamRef(StreamPath path, IStreamProvider provider = null)
        {
            Path = path;
            this.provider = provider;
        }

        [NonSerialized]
        IAsyncStream<object> endpoint;
        IAsyncStream<object> Endpoint
        {
            get
            {
                if (endpoint != null)
                    return endpoint;
                
                if (provider == null)
                    throw new InvalidOperationException($"StreamRef [{Path}] has not been bound to runtime");

                return endpoint = provider.GetStream<object>(Guid.Empty, Path.Id);
            }
        }

        public StreamPath Path { get; }
        
        /// <summary>
        /// Publishes given item to a stream.
        /// </summary>
        /// <param name="item">The item to be published</param>
        /// <param name="token">The optional stream sequence token</param>
        /// <returns>A Task that is completed when the item has been accepted</returns>
        public virtual Task Publish(object item, StreamSequenceToken token = null) => 
            Endpoint.OnNextAsync(item, token);

        /// <summary>
        /// Publishes given batch of items to a stream.
        /// </summary>
        /// <param name="batch">The items to be published as a batch</param>
        /// <param name="token">The optional stream sequence token</param>
        /// <returns>A Task that is completed when the batch has been accepted</returns>
        public virtual Task Publish(IEnumerable<object> batch, StreamSequenceToken token = null) => 
            Endpoint.OnNextBatchAsync(batch, token);

        /// <summary>
        /// Subscribe a consumer to this stream reference using weakly-typed delegate.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="filter">Optional items filter.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitly unsubscribed.
        /// </returns>
        public virtual async Task<StreamSubscription> Subscribe(Func<object, Task> callback, StreamFilter filter = null)
        {
            Requires.NotNull(callback, nameof(callback));

            var observer = new Observer((item, token) => callback(item));
            var predicate = filter != null ? StreamFilter.Internal.Predicate : (StreamFilterPredicate) null;
            var handle = await Endpoint.SubscribeAsync(observer, null, predicate, filter);

            return new StreamSubscription(handle);
        }
        
        /// <summary>
        /// The batch-oriented stream subscription using weakly-typed delegate.
        /// </summary>
        /// <param name="callback">Callback delegate to receive stream pushes in batches.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitly unsubscribed.
        /// </returns>
        public virtual async Task<StreamSubscription> Subscribe(Func<object[], Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            var observer = new BatchObserver(callback);
            var handle = await Endpoint.SubscribeAsync(observer);

            return new StreamSubscription(handle);
        }

        /// <summary>
        /// Subscribe a consumer to this stream reference using strongly-typed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the items produced by the stream.</typeparam>
        /// <param name="callback">Strongly-typed version of callback delegate.</param>
        /// <param name="filter">Optional items filter.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitly unsubscribed.
        /// </returns>
        public virtual Task<StreamSubscription> Subscribe<T>(Func<T, Task> callback, StreamFilter filter = null)
        {
            Requires.NotNull(callback, nameof(callback));

            return Subscribe(item => callback((T)item), filter);
        }

        /// <summary>
        /// Subscribe a consumer to this stream reference using weakly-typed delegate.
        /// </summary>
        /// <param name="callback">Callback delegate.</param>
        /// <param name="filter">Optional items filter.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitly unsubscribed.
        /// </returns>
        public virtual Task<StreamSubscription> Subscribe(Action<object> callback, StreamFilter filter = null)
        {
            Requires.NotNull(callback, nameof(callback));

            return Subscribe(item =>
            {
                callback(item);
                return Task.CompletedTask;
            },
            filter);
        }

        /// <summary>
        /// Subscribe a consumer to this stream reference using strongly-typed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the items produced by the stream.</typeparam>
        /// <param name="callback">Strongly-typed version of callback delegate.</param>
        /// <param name="filter">Optional items filter.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitly unsubscribed.
        /// </returns>
        public virtual Task<StreamSubscription> Subscribe<T>(Action<T> callback, StreamFilter filter = null)
        {
            Requires.NotNull(callback, nameof(callback));

            return Subscribe(item =>
            {
                callback((T)item);
                return Task.CompletedTask;
            }, 
            filter);
        }

        /// <summary>
        /// Returns a list of all current stream subscriptions.
        /// </summary>
        /// <returns> A promise for a list of StreamSubscription </returns>
        public virtual async Task<IList<StreamSubscription>> Subscriptions()
        {
            var handles = await Endpoint.GetAllSubscriptionHandles();
            return handles.Select(x => new StreamSubscription(x)).ToList();
        }

        public bool Equals(StreamRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other)
                    || Path.Equals(other.Path));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType() && Equals((StreamRef)obj));
        }

        public static implicit operator StreamPath(StreamRef arg) => arg.Path;

        public bool Equals(StreamPath other) => Path.Equals(other);
        public override int GetHashCode() => Path.GetHashCode();

        public static bool operator ==(StreamRef left, StreamRef right) => Equals(left, right);
        public static bool operator !=(StreamRef left, StreamRef right) => !Equals(left, right);

        public override string ToString() => Path.ToString();

        #region Orleans Native Serialization

        [CopierMethod]
        static object Copy(object input, ICopyContext context) => input;

        [SerializerMethod]
        static void Serialize(object input, ISerializationContext context, Type expected)
        {
            var writer = context.StreamWriter;
            var @ref = (StreamRef)input;
            writer.Write(@ref.Path);
        }

        [DeserializerMethod]
        static object Deserialize(Type t, IDeserializationContext context)
        {
            var reader = context.StreamReader;
            var path = StreamPath.Parse(reader.ReadString());
            var provider = context.ServiceProvider.GetServiceByName<IStreamProvider>(path.Provider);
            return new StreamRef(path, provider);
        }

        #endregion

        internal class BatchObserver : IAsyncBatchObserver<object>
        {
            readonly Func<object[], Task> callback;

            public BatchObserver(Func<object[], Task> callback) => 
                this.callback = callback;

            public Task OnNextAsync(IList<SequentialItem<object>> items) => 
                callback(items.Select(x => x.Item).ToArray());

            public Task OnCompletedAsync()           => Task.CompletedTask;
            public Task OnErrorAsync(Exception ex)   => Task.CompletedTask;
        }

        internal class Observer : IAsyncObserver<object>
        {
            readonly Func<object, StreamSequenceToken, Task> callback;

            public Observer(Func<object, StreamSequenceToken, Task> callback) => 
                this.callback = callback;

            public Task OnNextAsync(object item, StreamSequenceToken token = null) => 
                callback(item, token);

            public Task OnCompletedAsync()           => Task.CompletedTask;
            public Task OnErrorAsync(Exception ex)   => Task.CompletedTask;
        }
    }
}