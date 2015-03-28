using System;
using System.Linq;
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
    public class Observer : IObservable<object>, IDisposable
    {
        /// <summary>
        /// Creates new <see cref="Observer"/>
        /// </summary>
        /// <returns>New instance of <see cref="Observer"/></returns>
        public static async Task<Observer> Create()
        {
            var proxy = await ClientEndpoint.Create();
            return new Observer(proxy);
        }

        readonly ClientEndpoint endpoint;

        protected Observer(ObserverRef @ref)
        {
            Ref = @ref;
        }

        Observer(ClientEndpoint endpoint) 
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

        public static implicit operator ObserverRef(Observer arg)
        {
            return arg.Ref;
        }
    }

    public static class ObserverExtensions
    {
        /// <summary>
        ///   Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        ///   A reference to an interface that allows observers to stop receiving notifications before the provider has finished
        ///   sending them.
        /// </returns>
        /// <param name="client">The instance of client observable proxy</param>
        /// <param name="callback">The callback delegate that is to receive notifications</param>
        public static IDisposable Subscribe(this Observer client, Action<object> callback)
        {
            Requires.NotNull(callback, "callback");

            return client.Subscribe(new DelegateObserver(callback));
        }

        public static IDisposable Subscribe<T>(this Observer client, Action<T> callback)
        {
            Requires.NotNull(callback, "callback");

            return client.Subscribe(new DelegateObserver(x => callback((T)x)));
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