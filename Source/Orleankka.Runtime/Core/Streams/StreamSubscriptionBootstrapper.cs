using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Internals;
using Orleans.Runtime;
using Orleans.Storage;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka.Core.Streams
{
    using System;
    using System.Threading;

    using Microsoft.Extensions.Options;

    public class StreamSubscriptionBootstrapperOptions
    {
        public string[] Providers { get; set; }
    }

    /// <remarks>
    /// This is done as storage provider due to initialization order inside Silo.DoStart()
    /// </remarks>
    class StreamSubscriptionBootstrapper : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
    {
        public static IGrainStorage Create(IServiceProvider services, string name)
        {
            var options = services.GetService<IOptionsSnapshot<StreamSubscriptionBootstrapperOptions>>().Get(name);
            return new StreamSubscriptionBootstrapper(services, options.Providers);
        }

        readonly IActorSystem system;
        readonly IServiceProvider services;
        readonly string[] providers;

        StreamSubscriptionBootstrapper(IServiceProvider services, string[] providers)
        {
            system = services.GetRequiredService<IActorSystem>();
            this.services = services;
            this.providers = providers;
        }

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(OptionFormattingUtilities.Name<StreamSubscriptionBootstrapper>(), ServiceLifecycleStage.ApplicationServices, Init);
        }

        Task Init(CancellationToken cancellation)
        {
            StreamPubSubWrapper.Hook(services, providers, stream => 
                StreamSubscriptionMatcher
                    .Match(system, stream)
                    .Select(x => new StreamPubSubMatch(x.Receive))
                    .ToArray());

            return Task.CompletedTask;
        }
        
        #region Garbage

        public Logger Log  { get; set; }
        public string Name { get; set; }
        public Task Close() => Task.CompletedTask;
        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)  => Task.CompletedTask;
        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => Task.CompletedTask;
        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => Task.CompletedTask;

        #endregion
    }
}