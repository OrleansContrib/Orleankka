using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka.Core
{
    class ObserverEndpoint : IObserverEndpoint, IDisposable
    {
        public static async Task<ObserverEndpoint> Create()
        {
            var endpoint = new ObserverEndpoint();

            var proxy = await ObserverEndpointFactory
                .CreateObjectReference(endpoint);

            return endpoint.Initialize(proxy);
        }

        IObserverEndpoint proxy;
        IObserver<Notification> observer;

        ObserverEndpoint Initialize(IObserverEndpoint proxy)
        {
            this.proxy = proxy;
            
            Self = ObserverRef.Deserialize(IdentityOf(proxy));
            
            return this;
        }

        public ObserverRef Self
        {
            get; private set;
        }

        public void Dispose()
        {
            ObserverEndpointFactory.DeleteObjectReference(proxy);
        }

        public IDisposable Subscribe(IObserver<Notification> observer)
        {
            Requires.NotNull(observer, "observer");

            if (this.observer != null)
                throw new ArgumentException("Susbscription has already been registered", "observer");

            this.observer = observer;

            return new DisposableSubscription(this);
        }

        public void ReceiveNotify(NotificationEnvelope envelope)
        {
            if (observer == null)
                return;

            var @sender = ActorRef.Resolve(envelope.Sender);
            var notification = new Notification(@sender, envelope.Message);

            observer.OnNext(notification);
        }

        class DisposableSubscription : IDisposable
        {
            readonly ObserverEndpoint owner;

            public DisposableSubscription(ObserverEndpoint owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                owner.observer = null;
            }
        }

        static string IdentityOf(IObserverEndpoint proxy)
        {
            return ((GrainReference)proxy).ToKeyString();
        }

        internal static IObserverEndpoint Proxy(string path)
        {
            return ObserverEndpointFactory.Cast(GrainReference.FromKeyString(path));
        }
    }
}