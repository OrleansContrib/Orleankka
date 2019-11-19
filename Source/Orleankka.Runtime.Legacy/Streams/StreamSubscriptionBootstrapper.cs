using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Orleans;
using Orleans.Internals;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleankka.Legacy.Streams
{
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
        readonly StreamSubscriptionSpecificationRegistry subscriptions;
        readonly string[] providers;

        StreamSubscriptionBootstrapper(IServiceProvider services, string[] providers)
        {
            system = services.GetRequiredService<IActorSystem>();
            subscriptions = services.GetRequiredService<StreamSubscriptionSpecificationRegistry>();

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
                    .Match(system, stream.Id, subscriptions.Find(stream.Provider))
                    .Select(x => new StreamPubSubMatch(x.Receive))
                    .ToArray());

            return Task.CompletedTask;
        }
        
        #region Garbage

        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)  => Task.CompletedTask;
        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => Task.CompletedTask;
        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => Task.CompletedTask;

        #endregion
    }
}