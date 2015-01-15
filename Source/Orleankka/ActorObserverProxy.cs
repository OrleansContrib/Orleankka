using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    /// <summary>
    /// Allows clients to receive push-based notifications from actors, ie observing them.
    /// <para>
    /// To teardown created back-channel and delete underlying runtime reference call <see cref="IDisposable.Dispose"/>
    /// </para>
    /// </summary>
    /// <remarks> Instances of this type are not thread safe </remarks>
    public interface IActorObserverProxy : IObservable<Notification>, IDisposable
    {
        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.
        /// </returns>
        /// <param name="callback">The callback delegate that is to receive notifications.</param>
        IDisposable Subscribe(Action<Notification> callback);

        /// <summary>
        /// Gets the actual observer proxy that could be passed to <see cref="IActorRef.Handle"/>.
        /// </summary>
        /// <value>
        /// The observer proxy.
        /// </value>
        IActorObserver Proxy
        {
            get;
        }
    }

    /// <summary>
    /// Factory for <see cref="IActorObserverProxy"/>
    /// </summary>
    public sealed class ActorObserverProxy : IActorObserverProxy, IActorObserver
    {
        /// <summary>
        /// Creates new <see cref="IActorObserverProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IActorObserverProxy"/></returns>
        public static async Task<ActorObserverProxy> Create()
        {
            var instance = new ActorObserverProxy();

            var proxy = await ActorObserverFactory.CreateObjectReference(instance);
            instance.Initialize(proxy);

            return instance;
        }

        IActorObserver proxy;
        IObserver<Notification> observer;

        void Initialize(IActorObserver proxy)
        {
            this.proxy = proxy;
        }

        void IDisposable.Dispose()
        {
            ActorObserverFactory.DeleteObjectReference(proxy);
        }

        public IActorObserver Proxy
        {
            get { return proxy; }
        }

        IDisposable IActorObserverProxy.Subscribe(Action<Notification> callback)
        {
            Requires.NotNull(callback, "callback");

            return DoSubscribe(new DelegateObserver(callback));
        }

        IDisposable IObservable<Notification>.Subscribe(IObserver<Notification> observer)
        {
            Requires.NotNull(observer, "observer");

            return DoSubscribe(observer);
        }

        IDisposable DoSubscribe(IObserver<Notification> observer)
        {
            if (this.observer != null)
                throw new ArgumentException("Susbscription has already been registered", "observer");

            this.observer = observer;

            return new DisposableSubscription(this);
        }

        void IActorObserver.OnNext(Notification notification)
        {
            if (observer != null)
                observer.OnNext(notification);
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

        class DisposableSubscription : IDisposable
        {
            readonly ActorObserverProxy parent;

            public DisposableSubscription(ActorObserverProxy parent)
            {
                this.parent = parent;
            }

            public void Dispose()
            {
                parent.observer = null;
            }
        }
    }
}