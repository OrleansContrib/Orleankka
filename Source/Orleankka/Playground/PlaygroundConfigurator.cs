using System;
using System.Linq;

using Orleans.Runtime.Configuration;

namespace Orleankka.Playground
{
    using Client;
    using Cluster;
    using Embedded;

    public sealed class PlaygroundConfigurator : EmbeddedConfigurator
    {
        internal PlaygroundConfigurator(IActorSystemConfigurator configurator, AppDomainSetup setup)
            : base(configurator, setup)
        {}

        internal PlaygroundConfigurator Configure()
        {
            var cluster = new ClusterConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Cluster.xml");

            var client = new ClientConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Client.xml");

            From(client);
            From(cluster);

            cluster.Globals.LivenessType =
                GlobalConfiguration.LivenessProviderType.MembershipTableGrain;

            cluster.Globals.ReminderServiceType =
                GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;

            return this;
        }
    }

    public static class PlaygroundConfiguratorExtensions
    {
        public static PlaygroundConfigurator Playground(this ActorSystemConfigurator configurator, AppDomainSetup setup = null)
        {
            return new PlaygroundConfigurator(configurator, setup).Configure();
        }
    }
}