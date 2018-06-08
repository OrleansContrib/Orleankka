using System;
using System.Net;

using Orleans;
using Orleans.Hosting;
using Orleans.Configuration;

namespace Orleankka.Playground
{
    using Client;
    using Cluster;
    using Embedded;
    using Utility;

    public sealed class PlaygroundConfigurator : EmbeddedConfigurator
    {
        internal PlaygroundConfigurator()
        {
            Cluster(c =>
            {
                c.Builder(b => b.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "playground";
                    options.ServiceId = "playground";
                })
                .UseLocalhostClustering()
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .AddMemoryGrainStorage("PubSubStore")                
                .UseInMemoryReminderService());
            });

            Client(c =>
            {
                c.Builder(b => b.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "playground";
                    options.ServiceId = "playground";
                })
                .UseLocalhostClustering());
            });

            UseSimpleMessageStreamProvider("sms", o => o.Configure(x => x.FireAndForgetDelivery = false));
        }

        public new PlaygroundConfigurator Client(Action<ClientConfigurator> configure)
        {
            Requires.NotNull(configure, nameof(configure));
            base.Client(configure);
            return this;
        }

        public new PlaygroundConfigurator Cluster(Action<ClusterConfigurator> configure)
        {
            Requires.NotNull(configure, nameof(configure));
            base.Cluster(configure);
            return this;
        }
    }

    public static class PlaygroundConfiguratorExtensions
    {
        public static PlaygroundConfigurator Playground(this IActorSystemConfigurator root)
        {
            return new PlaygroundConfigurator();
        }
    }
}