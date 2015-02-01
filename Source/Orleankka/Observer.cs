using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;

    /// <summary>
    /// Allows clients to receive push-based notifications from actors, ie observing them.
    /// <para>
    /// To teardown created back-channel and delete underlying client endpoint, call <see cref="IDisposable.Dispose"/>
    /// </para>
    /// </summary>
    /// <remarks> Instances of this type are not thread safe </remarks>
    public class Observer : IObservable<Notification>, IDisposable
    {
        /// <summary>
        /// Creates new <see cref="Observer"/>
        /// </summary>
        /// <returns>New instance of <see cref="Observer"/></returns>
        public static async Task<Observer> Create()
        {
            var proxy = await ObserverEndpoint.Create();
            return new Observer(proxy);
        }

        readonly ObserverEndpoint endpoint;
        readonly ObserverRef @ref;

        protected Observer(ObserverRef @ref)
        {
            this.@ref = @ref;
        }

        Observer(ObserverEndpoint endpoint) 
            : this(endpoint.Self)
        {
            this.endpoint = endpoint;
        }

        public virtual void Dispose()
        {
            endpoint.Dispose();
        }

        public virtual IDisposable Subscribe(IObserver<Notification> observer)
        {
            return endpoint.Subscribe(observer);
        }

        public static implicit operator ObserverRef(Observer arg)
        {
            return arg.@ref;
        }
    }

    public static class ActorObserverProxyExtensions
    {
        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        /// <param name="client">The instance of client observable proxy</param>
        /// <param name="callback">The callback delegate that is to receive notifications</param>
        public static IDisposable Subscribe(this Observer client, Action<Notification> callback)
        {
            Requires.NotNull(callback, "callback");

            return client.Subscribe(new DelegateObserver(callback));
        }
       
        class DelegateObserver : IObserver<Notification>
        {
            readonly Action<Notification> callback;

            public DelegateObserver(Action<Notification> callback)
            {
                this.callback = callback;
            }

            public void OnNext(Notification value)
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