using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

namespace Orleankka.Client
{
    using Core;

    /// <summary>
    /// Client-side actor system interface
    /// </summary>
    public interface IClientActorSystem : IActorSystem
    {
        /// <summary>
        /// Creates new <see cref="IClientObservable"/>
        /// </summary>
        /// <returns>New instance of <see cref="IClientObservable"/></returns>
        Task<IClientObservable> CreateObservable();
    }

    /// <summary>
    /// Client-side actor system
    /// </summary>
    public sealed class ClientActorSystem : ActorSystem, IClientActorSystem
    {
        readonly IGrainFactory grainFactory;

        internal ClientActorSystem(
            IStreamProviderManager streamProviderManager, 
            IGrainFactory grainFactory, 
            IActorRefInvoker invoker = null)
            : base(streamProviderManager, grainFactory, invoker)
        {
            this.grainFactory = grainFactory;
        }

        /// <inheritdoc />
        public async Task<IClientObservable> CreateObservable()
        {
            var proxy = await ClientEndpoint.Create(grainFactory);
            return new ClientObservable(proxy);
        }
    }
}