using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Utility;

    public static class StreamRefExtensions
    {
        /// <summary>
        /// Subscribe a consumer to this stream reference using strongly-typed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the items produced by the stream.</typeparam>
        /// <param name="stream">The stream reference.</param>
        /// <param name="callback">Strongly-typed version of callback delegate.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscription> Subscribe<T>(this StreamRef stream, Func<StreamPath, T, Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return stream.Subscribe((source, item) => callback(source, (T) item));
        }
        
        /// <summary>
        /// Subscribe a consumer to this stream reference using strongly-typed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the items produced by the stream.</typeparam>
        /// <param name="stream">The stream reference.</param>
        /// <param name="callback">Strongly-typed version of callback delegate.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscription> Subscribe<T>(this StreamRef stream, Func<T, Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return stream.Subscribe((source, item) => callback((T) item));
        }

        /// <summary>
        /// Subscribe a consumer to this stream reference using strongly-typed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the items produced by the stream.</typeparam>
        /// <param name="stream">The stream reference.</param>
        /// <param name="callback">Strongly-typed version of callback delegate.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscription> Subscribe<T>(this StreamRef stream, Action<StreamPath, T> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return stream.Subscribe((source, item) =>
            {
                callback(source, (T)item);
                return TaskDone.Done;
            });
        }

        /// <summary>
        /// Subscribe a consumer to this stream reference using strongly-typed delegate.
        /// </summary>
        /// <typeparam name="T">The type of the items produced by the stream.</typeparam>
        /// <param name="stream">The stream reference.</param>
        /// <param name="callback">Strongly-typed version of callback delegate.</param>
        /// <returns>
        /// A promise for a StreamSubscription that represents the subscription.
        /// The consumer may unsubscribe by using this object.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscription> Subscribe<T>(this StreamRef stream, Action<T> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return stream.Subscribe((source, item) =>
            {
                callback((T) item);
                return TaskDone.Done;
            });
        }
    }
}
