using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Orleans.Concurrency;
using Orleans.Streams;

namespace Orleankka
{
    using Utility; 

    /// <summary>
    /// Represents handle to a stream subscription.
    /// </summary>
    /// <remarks>
    /// Consumers may store or serialize the handle
    /// in order to resume or unsubscribe later.
    /// </remarks>
    [Serializable, Immutable]
    [DebuggerDisplay("sub::{Stream.ToString()}")]
    public class StreamSubscription<TItem>
        : IEquatable<StreamSubscription<TItem>>
    {
        readonly StreamSubscriptionHandle<TItem> handle;

        protected internal StreamSubscription(StreamRef<TItem> stream, StreamSubscriptionHandle<TItem> handle, Guid? id = null)
        {
            this.handle = handle;
            Stream = stream;
            Id = id ?? handle.HandleId;
        }

        /// <summary>
        /// Unique identifier of the stream subscription
        /// </summary>
        public Guid Id { get; }
    
        /// <summary>
        /// The reference to a stream that
        /// this subscription is subscribed to
        /// </summary>
        public StreamRef<TItem> Stream { get; }

        /// <summary>
        /// Unsubscribe a stream consumer from the stream.
        /// </summary>
        /// <returns>A promise to await for subscription to be removed</returns>
        public virtual Task Unsubscribe() => handle.UnsubscribeAsync();

        /// <summary>
        /// Resumes receiving messages published to a stream via given consumer callback 
        /// </summary>
        /// <param name="callback">The callback delegate.</param>
        /// <param name="options">The stream resume options</param>
        /// <typeparam name="TOptions">The type of stream resume options</typeparam>
        /// <returns>
        /// A promise for a new <see cref="StreamSubscription{TItem}"/> that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitly unsubscribed.
        /// </returns>
        public virtual async Task<StreamSubscription<TItem>> Resume<TOptions>(Func<StreamMessage, Task> callback, TOptions options) 
            where TOptions : ResumeOptions
        {
            Requires.NotNull(callback, nameof(callback));

            return options switch 
            {
                ResumeReceiveBatch o => await ResumeBatch(o), 
                ResumeReceiveItem o  => await Resume(o),
                _ => throw new ArgumentOutOfRangeException(nameof(options), 
                    $"Unsupported type of options: '{options.GetType()}'")
            };

            async Task<StreamSubscription<TItem>> Resume(ResumeReceiveItem o)
            {
                var observer = Stream.CreateObserver(callback);
                return new StreamSubscription<TItem>(Stream, await handle.ResumeAsync(observer, o.Token));
            }

            async Task<StreamSubscription<TItem>> ResumeBatch(ResumeReceiveBatch o)
            {
                var observer = Stream.CreateBatchObserver(callback);
                return new StreamSubscription<TItem>(Stream, await handle.ResumeAsync(observer, o.Token));
            }
        }

        public bool Equals(StreamSubscription<TItem> other) =>
            !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || 
            handle.Equals(other.handle));

        public override bool Equals(object obj) =>
            !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || 
            obj.GetType() == this.GetType() && Equals((StreamSubscription<TItem>) obj));

        public override int GetHashCode() => handle.GetHashCode();

        public static bool operator ==(StreamSubscription<TItem> left, StreamSubscription<TItem> right) => Equals(left, right);
        public static bool operator !=(StreamSubscription<TItem> left, StreamSubscription<TItem> right) => !Equals(left, right);
    }
}