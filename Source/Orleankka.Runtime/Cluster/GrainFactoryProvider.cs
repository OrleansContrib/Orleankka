using System;
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
            ActorInterface.Bind(providerRuntime.GrainFactory);

            ClusterActorSystem.Current.StreamProviderManager = (IStreamProviderManager)providerRuntime.ServiceProvider.GetService(typeof(IStreamProviderManager));
            ClusterActorSystem.Current.GrainFactory = providerRuntime.GrainFactory;

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
