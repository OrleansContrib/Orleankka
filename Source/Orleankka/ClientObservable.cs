using System;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;
    using Utility;

    /// <summary>
    /// Allows clients to receive push-based notifications from actors, ie observing them.
    /// <para>
    /// To teardown created back-channel and delete underlying client endpoint, call <see cref="IDisposable.Dispose"/>
    /// </para>
    /// </summary>
    /// <remarks> Instances of this type are not thread safe </remarks>
    public class ClientObservable : IObservable<object>, IDisposable
    {
        /// <summary>
        /// Creates new <see cref="ClientObservable"/>
        /// </summary>
        /// <returns>New instance of <see cref="ClientObservable"/></returns>
        public static async Task<ClientObservable> Create()
        {
            var proxy = await ClientEndpoint.Create();
            return new ClientObservable(proxy);
        }

        readonly ClientEndpoint endpoint;

        protected ClientObservable(ObserverRef @ref)
        {
            Ref = @ref;
        }

        ClientObservable(ClientEndpoint endpoint) 
            : this(endpoint.Self)
        {
            this.endpoint = endpoint;
        }

        public ObserverRef Ref
        {
            get; private set;
        }

        public virtual void Dispose()
        {
            endpoint.Dispose();
        }

        public virtual IDisposable Subscribe(IObserver<object> observer)
        {
            return endpoint.Subscribe(observer);
        }

        public static implicit operator ObserverRef(ClientObservable arg)
        {
            return arg.Ref;
        }
    }

    public static class ClientObservableExtensions
    {
        /// <summary>
        ///   Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        ///   A reference to an interface that allows observers to stop receiving notifications before the provider has finished
        ///   sending them.
        /// </returns>
        /// <param name="observable">The instance of client observable proxy</param>
        /// <param name="callback">The callback delegate that is to receive notifications</param>
        public static IDisposable Subscribe(this ClientObservable observable, Action<object> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return observable.Subscribe(new DelegateObserver(callback));
        }

        public static IDisposable Subscribe<T>(this ClientObservable observable, Action<T> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return observable.Subscribe(new DelegateObserver(x => callback((T)x)));
        }

        class DelegateObserver : IObserver<object>
        {
            readonly Action<object> callback;

            public DelegateObserver(Action<object> callback)
            {
                this.callback = callback;
            }

            public void OnNext(object value)
            {
                callback(value);
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }
        }
    }
}