using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Runtime;
using Orleankka.Dynamic.Internal;

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
        /// <summary>
        /// Creates new <see cref="ClientObservable"/>
        /// </summary>
        /// <returns>New instance of <see cref="ClientObservable"/></returns>
        public static async Task<ClientObservable> Create()
        {
            var observer = new ClientActorObserver();

            var actorObserverProxy = await ActorObserverFactory.CreateObjectReference(observer);
            var dynamicActorObserverProxy = await DynamicActorObserverFactory.CreateObjectReference(observer);

            return new ClientObservable(observer, actorObserverProxy, dynamicActorObserverProxy);
        }

        readonly ClientActorObserver client;
        readonly IActorObserver actorObserverProxy;
        readonly IDynamicActorObserver dynamicActorObserverProxy;
        readonly ActorPath path;

        protected ClientObservable(ActorPath path)
        {
            this.path = path;
        }

        ClientObservable(ClientActorObserver client, IActorObserver actorObserverProxy, IDynamicActorObserver dynamicActorObserverProxy)
            : this(new ActorPath(typeof(ClientObservable), IdentityOf(actorObserverProxy, dynamicActorObserverProxy)))
        {
            this.client = client;
            this.actorObserverProxy = actorObserverProxy;
            this.dynamicActorObserverProxy = dynamicActorObserverProxy;
        }

        public virtual void Dispose()
        {
            ActorObserverFactory.DeleteObjectReference(actorObserverProxy);
            DynamicActorObserverFactory.DeleteObjectReference(dynamicActorObserverProxy);
        }

        /// <summary>
        /// <para>
        /// Gets the runtime path of the underlying <see cref="IActorObserver"/>  proxy that could be passed (serialized) along with the message.
        /// </para>
        /// The path could be dehydrated back into a reference of <see cref="IActorObserver"/> interface by using <see cref="IActorSystem.ObserverOf"/> method.
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

        class ClientActorObserver : IActorObserver, IDynamicActorObserver
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

            public void OnNext(DynamicNotification notification)
            {
                OnNext(new Notification(notification.Source, notification.Message));
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

        static readonly string[] keySeparator = {"++"};

        static string IdentityOf(IActorObserver actorObserverProxy, IDynamicActorObserver dynamicActorObserverProxy)
        {
            var observerKey = ((GrainReference)actorObserverProxy).ToKeyString();
            var dynamicObserverKey = ((GrainReference)dynamicActorObserverProxy).ToKeyString();
            return string.Format("{0}{1}{2}", observerKey, keySeparator[0], dynamicObserverKey);
        }

        internal static bool IsCompatible(ActorPath path)
        {
            return path.Type == typeof(ClientObservable);
        }

        internal static IActorObserver Observer(ActorPath path)
        {
            return ActorObserverFactory.Cast(ObserverReference(path));
        }

        internal static IDynamicActorObserver DynamicObserver(ActorPath path)
        {
            return DynamicActorObserverFactory.Cast(DynamicObserverReference(path));
        }

        static GrainReference ObserverReference(ActorPath path)
        {
            return Reference(Keys(path)[0]);
        }

        static GrainReference DynamicObserverReference(ActorPath path)
        {
            return Reference(Keys(path)[1]);
        }

        static string[] Keys(ActorPath path)
        {
            return path.Id.Split(keySeparator, 2, StringSplitOptions.None);
        }

        static GrainReference Reference(string key)
        {
            return GrainReference.FromKeyString(key);
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