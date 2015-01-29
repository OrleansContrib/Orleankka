using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka
{
    /// <summary>
    /// Allows clients to receive push-based notifications from actors, ie observing them.
    /// <para>
    /// To teardown created back-channel and delete underlying runtime reference call <see cref="IDisposable.Dispose"/>
    /// </para>
    /// </summary>
    /// <remarks> Instances of this type are not thread safe </remarks>
    public class ClientObservable : IObservable<Notification>, IEquatable<ActorPath>, IDisposable
    {
        const string TypeCode = "client";

        /// <summary>
        /// Creates new <see cref="ClientObservable"/>
        /// </summary>
        /// <returns>New instance of <see cref="ClientObservable"/></returns>
        public static async Task<ClientObservable> Create()
        {
            var observer = new ClientActorObserver();
            var proxy = await ActorObserverFactory.CreateObjectReference(observer);
            return new ClientObservable(observer, proxy);
        }

        readonly ClientActorObserver client;
        readonly IActorObserver proxy;
        readonly ActorPath path;
        
        protected ClientObservable(ActorPath path)
        {
            this.path = path;
        }

        ClientObservable(ClientActorObserver client, IActorObserver proxy)
            : this(new ActorPath(TypeCode, IdentityOf(proxy)))
        {
            this.client = client;
            this.proxy = proxy;
        }

        public virtual void Dispose()
        {
            ActorObserverFactory.DeleteObjectReference(proxy);
        }

        /// <summary>
        /// <para>
        /// Gets the runtime path of the underlying <see cref="IObserver{T}"/>  proxy that could be passed (serialized) along with the message.
        /// </para>
        /// The path could be dehydrated back into a reference of <see cref="IObserver{T}"/> interface by using <see cref="IActorSystem.ObserverOf"/> method.
        /// </summary>
        /// <value>
        /// The runtime path of the underlying observer proxy.
        /// </value>
        public ActorPath Path
        {
            get { return path; }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ClientObservable"/> to its <see cref="ActorPath"/> runtime path.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>
        /// The runtime path of the underlying observer proxy.
        /// </returns>
        public static implicit operator ActorPath(ClientObservable arg)
        {
            return arg.Path;
        }

        bool IEquatable<ActorPath>.Equals(ActorPath other)
        {
            return path.Equals(other);
        }
        
        public virtual IDisposable Subscribe(IObserver<Notification> observer)
        {
            return client.Subscribe(observer);
        }

        class ClientActorObserver : IActorObserver
        {
            IObserver<Notification> observer;

            public IDisposable Subscribe(IObserver<Notification> observer)
            {
                Requires.NotNull(observer, "observer");

                if (this.observer != null)
                    throw new ArgumentException("Susbscription has already been registered", "observer");

                this.observer = observer;

                return new DisposableSubscription(this);
            }

            public void OnNext(Notification notification)
            {
                if (observer != null)
                    observer.OnNext(notification);
            }

            class DisposableSubscription : IDisposable
            {
                readonly ClientActorObserver owner;

                public DisposableSubscription(ClientActorObserver owner)
                {
                    this.owner = owner;
                }

                public void Dispose()
                {
                    owner.observer = null;
                }
            }
        }

        static string IdentityOf(IActorObserver proxy)
        {
            return ((GrainReference)proxy).ToKeyString();
        }

        internal static bool IsCompatible(ActorPath path)
        {
            return path.TypeCode == TypeCode;
        }

        internal static IActorObserver Observer(ActorPath path)
        {
            return ActorObserverFactory.Cast(GrainReference.FromKeyString(path.Id));
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
        public static IDisposable Subscribe(this ClientObservable client, Action<Notification> callback)
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