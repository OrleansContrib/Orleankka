using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

namespace Orleankka
{
    using Utility;

    public static class AsyncObservableExtensions
    {
        static readonly Func<Exception, Task> DefaultOnError = _ => TaskDone.Done;
        static readonly Func<Task> DefaultOnCompleted = () => TaskDone.Done;

        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onErrorAsync">Delegte that is called for IAsyncObserver.OnErrorAsync.</param>
        /// <param name="onCompletedAsync">Delegte that is called for IAsyncObserver.OnCompletedAsync.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.</returns>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync,
                                                                           Func<Exception, Task> onErrorAsync,
                                                                           Func<Task> onCompletedAsync)
        {
            var genericObserver = new DelegateAsyncObserver<T>(onNextAsync, onErrorAsync, onCompletedAsync);
            return obs.SubscribeAsync(genericObserver);
        }

        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onErrorAsync">Delegte that is called for IAsyncObserver.OnErrorAsync.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.</returns>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync,
                                                                           Func<Exception, Task> onErrorAsync)
        {
            return obs.SubscribeAsync(onNextAsync, onErrorAsync, DefaultOnCompleted);
        }

        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onCompletedAsync">Delegte that is called for IAsyncObserver.OnCompletedAsync.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.</returns>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync,
                                                                           Func<Task> onCompletedAsync)
        {
            return obs.SubscribeAsync(onNextAsync, DefaultOnError, onCompletedAsync);
        }

        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.</returns>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync)
        {
            return obs.SubscribeAsync(onNextAsync, DefaultOnError, DefaultOnCompleted);
        }


        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onErrorAsync">Delegte that is called for IAsyncObserver.OnErrorAsync.</param>
        /// <param name="onCompletedAsync">Delegte that is called for IAsyncObserver.OnCompletedAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <param name="filterFunc">Filter to be applied for this subscription</param>
        /// <param name="filterData">Data object that will be passed in to the filterFunc.
        /// This will usually contain any paramaters required by the filterFunc to make it's filtering decision.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the supplied stream filter function is not suitable. 
        /// Usually this is because it is not a static method. </exception>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync,
                                                                           Func<Exception, Task> onErrorAsync,
                                                                           Func<Task> onCompletedAsync,
                                                                           StreamSequenceToken token,
                                                                           StreamFilterPredicate filterFunc = null,
                                                                           object filterData = null)
        {
            var genericObserver = new DelegateAsyncObserver<T>(onNextAsync, onErrorAsync, onCompletedAsync);
            return obs.SubscribeAsync(genericObserver, token, filterFunc, filterData);
        }

        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onErrorAsync">Delegte that is called for IAsyncObserver.OnErrorAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <param name="filterFunc">Filter to be applied for this subscription</param>
        /// <param name="filterData">Data object that will be passed in to the filterFunc.
        /// This will usually contain any paramaters required by the filterFunc to make it's filtering decision.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the supplied stream filter function is not suitable. 
        /// Usually this is because it is not a static method. </exception>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync,
                                                                           Func<Exception, Task> onErrorAsync,
                                                                           StreamSequenceToken token,
                                                                           StreamFilterPredicate filterFunc = null,
                                                                           object filterData = null)
        {
            return obs.SubscribeAsync(onNextAsync, onErrorAsync, DefaultOnCompleted, token, filterFunc, filterData);
        }

        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onCompletedAsync">Delegte that is called for IAsyncObserver.OnCompletedAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <param name="filterFunc">Filter to be applied for this subscription</param>
        /// <param name="filterData">Data object that will be passed in to the filterFunc.
        /// This will usually contain any paramaters required by the filterFunc to make it's filtering decision.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the supplied stream filter function is not suitable. 
        /// Usually this is because it is not a static method. </exception>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync,
                                                                           Func<Task> onCompletedAsync,
                                                                           StreamSequenceToken token,
                                                                           StreamFilterPredicate filterFunc = null,
                                                                           object filterData = null)
        {
            return obs.SubscribeAsync(onNextAsync, DefaultOnError, onCompletedAsync, token, filterFunc, filterData);
        }

        /// <summary>
        /// Subscribe a consumer to this observable using delegates.
        /// This method is a helper for the IAsyncObservable.SubscribeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="obs">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <param name="filterFunc">Filter to be applied for this subscription</param>
        /// <param name="filterData">Data object that will be passed in to the filterFunc.
        /// This will usually contain any paramaters required by the filterFunc to make it's filtering decision.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the supplied stream filter function is not suitable. 
        /// Usually this is because it is not a static method. </exception>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> obs,
                                                                           Func<T, StreamSequenceToken, Task> onNextAsync,
                                                                           StreamSequenceToken token,
                                                                           StreamFilterPredicate filterFunc = null,
                                                                           object filterData = null)
        {
            return obs.SubscribeAsync(onNextAsync, DefaultOnError, DefaultOnCompleted, token, filterFunc, filterData);
        }

        /// <summary>
        /// Subscribe a consumer to this observable.
        /// </summary>
        /// <param name="observable">An observable</param>
        /// <param name="observer">The asynchronous observer to subscribe.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> observable, 
                                                                               IAsyncObserver<T> observer)
        {
            return observable.SubscribeAsync(new GenericAsyncObserver<T>(observer));
        }

        /// <summary>
        /// Subscribe a consumer to this observable.
        /// </summary>
        /// <param name="observable">An observable</param>
        /// <param name="observer">The asynchronous observer to subscribe.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <param name="filterFunc">Filter to be applied for this subscription</param>
        /// <param name="filterData">Data object that will be passed in to the filterFunc.
        /// This will usually contain any paramaters required by the filterFunc to make it's filtering decision.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the supplied stream filter function is not suitable. 
        /// Usually this is because it is not a static method. </exception>
        public static Task<StreamSubscriptionHandle<object>> SubscribeAsync<T>(this IAsyncObservable<object> observable, 
                                                                               IAsyncObserver<T> observer, 
                                                                               StreamSequenceToken token, 
                                                                               StreamFilterPredicate filterFunc = null, 
                                                                               object filterData = null)
        {
            return observable.SubscribeAsync(new GenericAsyncObserver<T>(observer), token, filterFunc, filterData);
        }
    }

    public static class StreamSubscriptionHandleExtensions
    {
        private static readonly Func<Exception, Task> DefaultOnError = _ => TaskDone.Done;
        private static readonly Func<Task> DefaultOnCompleted = () => TaskDone.Done;

        /// <summary>
        /// Resumes consumption of a stream using delegates.
        /// This method is a helper for the StreamSubscriptionHandle.ResumeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="handle">The Observable object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onErrorAsync">Delegte that is called for IAsyncObserver.OnErrorAsync.</param>
        /// <param name="onCompletedAsync">Delegte that is called for IAsyncObserver.OnCompletedAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscriptionHandle<object>> ResumeAsync<T>(
            this StreamSubscriptionHandle<object> handle,
            Func<T, StreamSequenceToken, Task> onNextAsync,
            Func<Exception, Task> onErrorAsync,
            Func<Task> onCompletedAsync,
            StreamSequenceToken token = null)
        {
            var genericObserver = new DelegateAsyncObserver<T>(onNextAsync, onErrorAsync, onCompletedAsync);
            return handle.ResumeAsync(genericObserver, token);
        }

        /// <summary>
        /// Resumes consumption of a stream using delegates.
        /// This method is a helper for the StreamSubscriptionHandle.ResumeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="handle">The StreamSubscriptionHandle object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onErrorAsync">Delegte that is called for IAsyncObserver.OnErrorAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscriptionHandle<object>> ResumeAsync<T>(
            this StreamSubscriptionHandle<object> handle,
            Func<T, StreamSequenceToken, Task> onNextAsync,
            Func<Exception, Task> onErrorAsync,
            StreamSequenceToken token = null)
        {
            return handle.ResumeAsync(onNextAsync, onErrorAsync, DefaultOnCompleted, token);
        }

        /// <summary>
        /// Resumes consumption of a stream using delegates.
        /// This method is a helper for the StreamSubscriptionHandle.ResumeAsync allowing the subscribing class to inline the 
        /// handler methods instead of requiring an instance of IAsyncObserver.
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="handle">The StreamSubscriptionHandle object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="onCompletedAsync">Delegte that is called for IAsyncObserver.OnCompletedAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscriptionHandle<object>> ResumeAsync<T>(
            this StreamSubscriptionHandle<object> handle,
            Func<T, StreamSequenceToken, Task> onNextAsync,
            Func<Task> onCompletedAsync,
            StreamSequenceToken token = null)
        {
            return handle.ResumeAsync(onNextAsync, DefaultOnError, onCompletedAsync, token);
        }

        /// <summary>
        /// <exception cref="ArgumentException">Thrown if the supplied stream filter function is not suitable. 
        /// Usually this is because it is not a static method. </exception>
        /// </summary>
        /// <typeparam name="T">The type of object produced by the observable.</typeparam>
        /// <param name="handle">The StreamSubscriptionHandle object.</param>
        /// <param name="onNextAsync">Delegte that is called for IAsyncObserver.OnNextAsync.</param>
        /// <param name="token">The stream sequence to be used as an offset to start the subscription from.</param>
        /// <returns>A promise for a StreamSubscriptionHandle that represents the subscription.
        /// The consumer may unsubscribe by using this handle.
        /// The subscription remains active for as long as it is not explicitely unsubscribed.
        /// </returns>
        public static Task<StreamSubscriptionHandle<object>> ResumeAsync<T>(
            this StreamSubscriptionHandle<object> handle,
            Func<T, StreamSequenceToken, Task> onNextAsync,
            StreamSequenceToken token = null)
        {
            return handle.ResumeAsync(onNextAsync, DefaultOnError, DefaultOnCompleted, token);
        }

        /// <summary>
        /// Resumes consumption from a subscription to a stream.
        /// </summary>
        /// <param name="handle">The stream handle to consume from.</param>
        /// <param name="observer">The observer reference</param>
        /// <param name="token">The (optional) stream sequence token to be used as an offset to start the subscription from.</param>
        /// <returns>A promise with an updates subscription handle.
        /// </returns>
        public static Task<StreamSubscriptionHandle<object>> ResumeAsync<T>(
            this StreamSubscriptionHandle<object> handle,
            IAsyncObserver<T> observer, 
            StreamSequenceToken token = null)
        {
            return handle.ResumeAsync(new GenericAsyncObserver<T>(observer), token);
        }
    }

    class DelegateAsyncObserver<T> : IAsyncObserver<object>
    {
        readonly Func<T, StreamSequenceToken, Task> onNextAsync;
        readonly Func<Exception, Task> onErrorAsync;
        readonly Func<Task> onCompletedAsync;

        public DelegateAsyncObserver(Func<T, StreamSequenceToken, Task> onNextAsync, Func<Exception, Task> onErrorAsync, Func<Task> onCompletedAsync)
        {
            Requires.NotNull(onNextAsync, "onNextAsync");
            Requires.NotNull(onErrorAsync, "onErrorAsync");
            Requires.NotNull(onCompletedAsync, "onCompletedAsync");

            this.onNextAsync = onNextAsync;
            this.onErrorAsync = onErrorAsync;
            this.onCompletedAsync = onCompletedAsync;
        }

        public Task OnNextAsync(object item, StreamSequenceToken token = null)
        {
            return onNextAsync((T)item, token);
        }

        public Task OnCompletedAsync()
        {
            return onCompletedAsync();
        }

        public Task OnErrorAsync(Exception ex)
        {
            return onErrorAsync(ex);
        }
    }

    class GenericAsyncObserver<T> : IAsyncObserver<object>
    {
        readonly IAsyncObserver<T> inner;

        public GenericAsyncObserver(IAsyncObserver<T> inner)
        {
            Requires.NotNull(inner, "inner");
            this.inner = inner;
        }

        public Task OnNextAsync(object item, StreamSequenceToken token = null)
        {
            return inner.OnNextAsync((T)item, token);
        }

        public Task OnCompletedAsync()
        {
            return inner.OnCompletedAsync();
        }

        public Task OnErrorAsync(Exception ex)
        {
            return inner.OnErrorAsync(ex);
        }
    }
}
