using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Orleankka.Utility;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    partial class ClientEndpoint : IClientEndpoint, IDisposable
    {
        public static ClientEndpoint Create(IGrainFactory factory)
        {
            var endpoint = new ClientEndpoint(factory);

            var proxy = factory.CreateObjectReference<IClientEndpoint>(endpoint);

            return endpoint.Initialize(proxy);
        }

        readonly IGrainFactory factory;
        IClientEndpoint proxy;
        IObserver<object> observer;

        ClientEndpoint(IGrainFactory factory)
        {
            this.factory = factory;
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
            factory.DeleteObjectReference<IClientEndpoint>(proxy);
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

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        internal static string Path(IClientEndpoint proxy) => 
            ((GrainReference)proxy).GrainId.ToString();

        internal static IClientEndpoint Proxy(string path, IGrainFactory factory)
        {
            var reference = GrainReferenceInternals.FromKeyString(path);
            return reference.AsReference<IClientEndpoint>();
        }
    }
}