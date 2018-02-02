using System.Reflection;
using System.Threading.Tasks;

using Orleans;
using Orleans.ApplicationParts;
using Orleans.Streams;

namespace Orleankka.Client
{
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
            Assembly[] assemblies,
            IStreamProviderManager streamProviderManager,
            IGrainFactory grainFactory,
            IActorRefMiddleware middleware = null)
            : base(assemblies, streamProviderManager, grainFactory, middleware)
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