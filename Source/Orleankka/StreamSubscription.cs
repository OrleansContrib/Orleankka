using System;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka
{
    using Utility; 

    public class StreamSubscription<TItem>
    {
        readonly StreamRef<TItem> stream;
        readonly StreamSubscriptionHandle<TItem> handle;

        protected internal StreamSubscription(StreamRef<TItem> stream, StreamSubscriptionHandle<TItem> handle)
        {
            this.stream = stream;
            this.handle = handle;
        }

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
                var observer = new StreamRef<TItem>.Observer(stream, callback);
                return new StreamSubscription<TItem>(stream, await handle.ResumeAsync(observer, o.Token));
            }

            async Task<StreamSubscription<TItem>> ResumeBatch(ResumeReceiveBatch o)
            {
                var observer = new StreamRef<TItem>.BatchObserver(stream, callback);
                return new StreamSubscription<TItem>(stream, await handle.ResumeAsync(observer, o.Token));
            }
        }
    }
}