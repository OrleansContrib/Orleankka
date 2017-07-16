using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Core
{
    using Utility;

    class ClientEndpoint : IClientEndpoint, IDisposable
    {
        public static async Task<ClientEndpoint> Create(IClusterClient client)
        {
            var endpoint = new ClientEndpoint(client);

            var proxy = await client.CreateObjectReference<IClientEndpoint>(endpoint);

            return endpoint.Initialize(proxy);
        }

        readonly IClusterClient client;
        IClientEndpoint proxy;
        IObserver<object> observer;

        ClientEndpoint(IClusterClient client)
        {
            this.client = client;
        }

        ClientEndpoint Initialize(IClientEndpoint proxy)
        {
            this.proxy = proxy;
            
            Self = new ClientRef(proxy);
            
            return this;
        }

        public ClientRef Self
        {
            get; private set;
        }

        public void Dispose()
        {
            client.DeleteObjectReference<IClientEndpoint>(proxy);
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
    }
}