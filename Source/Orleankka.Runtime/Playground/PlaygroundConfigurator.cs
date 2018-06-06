using System;

using Orleans.Runtime.Configuration;

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
            var client = new ClientConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Client.xml");

            var cluster = new ClusterConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Cluster.xml");

            client.ClusterId = "playground";
            cluster.Globals.ClusterId = "playground";

            cluster.Globals.LivenessType =
                GlobalConfiguration.LivenessProviderType.MembershipTableGrain;

            cluster.Globals.ReminderServiceType =
                GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;

            UseSimpleMessageStreamProvider("sms", o => o.Configure(x => x.FireAndForgetDelivery = false));

            Cluster(c => c.From(cluster));
            Client(c => c.From(client));
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