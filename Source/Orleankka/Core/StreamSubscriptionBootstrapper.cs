using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Internals;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleankka.Core
{
    using Cluster;

    /// <summary> 
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    /// <remarks>
    /// This is done as storage provider due to initialization order inside Silo.DoStart()
    /// </remarks>
    public class StreamSubscriptionBootstrapper : IStorageProvider
    {
        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            var system = ClusterActorSystem.Current;
            var providers = config.Properties["providers"].Split(';');

            StreamPubSubWrapper.Hook(providers, stream => 
                StreamSubscriptionMatcher
                    .Match(system, stream)
                    .Select(x => new StreamPubSubMatch(x, x.Tell))
                    .ToArray());

            return TaskDone.Done;
        }

        #region Garbage

        public string Name { get; private set; }

        public Task Close()
        {
            return TaskDone.Done;
        }

        public Task ReadStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            throw new NotImplementedException();
        }

        public Task WriteStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            throw new NotImplementedException();
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            throw new NotImplementedException();
        }

        public Logger Log { get; }

        #endregion
    }
}