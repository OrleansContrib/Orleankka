using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka.Core
{
    using Utility;

    class ClientEndpoint : IClientEndpoint, IDisposable
    {
        public static async Task<ClientEndpoint> Create()
        {
            var endpoint = new ClientEndpoint();

            var proxy = await GrainClient.GrainFactory
                .CreateObjectReference<IClientEndpoint>(endpoint);

            return endpoint.Initialize(proxy);
        }

        IClientEndpoint proxy;
        IObserver<object> observer;

        ClientEndpoint Initialize(IClientEndpoint proxy)
        {
            this.proxy = proxy;
            
            Self = ClientRef.Deserialize(IdentityOf(proxy));
            
            return this;
        }

        public ClientRef Self
        {
            get; private set;
        }

        public void Dispose()
        {
            GrainClient.GrainFactory.DeleteObjectReference<IClientEndpoint>(proxy);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            Requires.NotNull(observer, nameof(observer));

            if (this.observer != null)
                throw new ArgumentException("Susbscription has already been registered", nameof(observer));

            this.observer = observer;

            return new DisposableSubscription(this);
        }

        public void Receive(object message)
        {
            observer?.OnNext(message);
        }

        class DisposableSubscription : IDisposable
        {
            readonly ClientEndpoint owner;

            public DisposableSubscription(ClientEndpoint owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                owner.observer = null;
            }
        }

        static string IdentityOf(IClientEndpoint proxy)
        {
            return ((GrainReference)proxy).ToKeyString();
        }

        internal static IClientEndpoint Proxy(string path)
        {
            // TODO: Fixit
            return null;
        }
    }
}