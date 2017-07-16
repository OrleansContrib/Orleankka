using System.Threading.Tasks;

using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.Streams;

namespace Orleankka.Cluster
{
    using Core;

    class GrainFactoryProvider : IStorageProvider
    {
        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            var factory = providerRuntime.GrainFactory;
            ActorInterface.Bind(factory);

            var serviceProvider = providerRuntime.ServiceProvider;
            var streamProviderManager = (IStreamProviderManager)serviceProvider.GetService(typeof(IStreamProviderManager));
            ClusterActorSystem.Current.StreamProvider = x => (IStreamProvider)streamProviderManager.GetProvider(x);
            ClusterActorSystem.Current.GrainFactory = (IGrainFactory)serviceProvider.GetService(typeof(IGrainFactory));

            return TaskDone.Done;
        }

        #region Garbage

        public Task Close() => TaskDone.Done;

        public string Name { get; }
        public Logger Log { get; }
        
        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => TaskDone.Done;
        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => TaskDone.Done;
        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => TaskDone.Done;


        #endregion
    }
}
