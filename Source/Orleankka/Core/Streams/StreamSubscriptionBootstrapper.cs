using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Internals;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleankka.Core.Streams
{
    using Cluster;

    /// <remarks>
    /// This is done as storage provider due to initialization order inside Silo.DoStart()
    /// </remarks>
    class StreamSubscriptionBootstrapper : IStorageProvider
    {
        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            var system = ClusterActorSystem.Current;
            var providers = config.Properties["providers"].Split(';');

            StreamPubSubWrapper.ListTypes();
            StreamPubSubWrapper.Hook(providers, stream => 
                StreamSubscriptionMatcher
                    .Match(system, stream)
                    .Select(x => new StreamPubSubMatch(x.Receive))
                    .ToArray());

            return TaskDone.Done;
        }

        #region Garbage

        public Logger Log  { get; set; }
        public string Name { get; set; }
        public Task Close() => TaskDone.Done;
        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)  => TaskDone.Done;
        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => TaskDone.Done;
        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => TaskDone.Done;

        #endregion
    }
}