using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Internals;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleankka.Core.Streams
{
    /// <remarks>
    /// This is done as storage provider due to initialization order inside Silo.DoStart()
    /// </remarks>
    class StreamSubscriptionBootstrapper : IStorageProvider
    {
        public Task Init(string name, IProviderRuntime runtime, IProviderConfiguration configuration)
        {
            var system = runtime.ServiceProvider.GetRequiredService<IActorSystem>();
            var providers = configuration.Properties["providers"].Split(';');

            StreamPubSubWrapper.Hook(runtime.ServiceProvider, providers, stream => 
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