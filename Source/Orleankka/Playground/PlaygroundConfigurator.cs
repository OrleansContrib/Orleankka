using System;
using System.Linq;

using Orleans.Runtime.Configuration;

namespace Orleankka.Playground
{
    using Client;
    using Cluster;
    using Embedded;
    using Utility;

    public sealed class PlaygroundConfigurator : EmbeddedConfigurator
    {
        readonly ClusterConfiguration cluster;
        readonly ClientConfiguration client;

        internal PlaygroundConfigurator(AppDomainSetup setup)
            : base(setup)
        {
            client = new ClientConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Client.xml");

            cluster = new ClusterConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Cluster.xml");

            cluster.Globals.LivenessType =
                GlobalConfiguration.LivenessProviderType.MembershipTableGrain;

            cluster.Globals.ReminderServiceType =
                GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;   
        }

        public PlaygroundConfigurator Tweak(Action<ClientConfiguration> tweak)
        {
            Requires.NotNull(tweak, "tweak");
            tweak(client);
            return this;
        }

        public PlaygroundConfigurator Tweak(Action<ClusterConfiguration> tweak)
        {
            Requires.NotNull(tweak, "tweak");
            tweak(cluster);
            return this;
        }

        public override IActorSystem Done()
        {
            From(client);
            From(cluster);

            return base.Done();
        }
    }

    public static class PlaygroundConfiguratorExtensions
    {
        public static PlaygroundConfigurator Playground(this IActorSystemConfigurator root, AppDomainSetup setup = null)
        {
            return new PlaygroundConfigurator(setup);
        }
    }
}