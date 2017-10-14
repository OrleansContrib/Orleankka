using System;
using System.Linq;
using System.Collections.Generic;

using Orleans.Storage;
using Orleans.Runtime.Configuration;
using Orleans.Providers.Streams.SimpleMessageStream;
using Orleans.Streams;

namespace Orleankka.Playground
{
    using Client;
    using Cluster;
    using Embedded;
    using Utility;

    public sealed class PlaygroundConfigurator : EmbeddedConfigurator
    {
        readonly ClusterConfiguration cluster;

        internal PlaygroundConfigurator()
        {
            var client = new ClientConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Client.xml");

            cluster = new ClusterConfiguration()
                .LoadFromEmbeddedResource<PlaygroundConfigurator>("Cluster.xml");

            cluster.Globals.LivenessType =
                GlobalConfiguration.LivenessProviderType.MembershipTableGrain;

            cluster.Globals.ReminderServiceType =
                GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;

            StreamProvider<SimpleMessageStreamProvider>("sms", new Dictionary<string, string> {{"FireAndForgetDelivery", "false"}});

            base.Cluster(c => c.From(cluster));
            base.Client(c => c.From(client));
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

        public PlaygroundConfigurator UseInMemoryPubSubStore()
        {
            RegisterPubSubStorageProvider<MemoryStorage>();
            return this;
        }

        void RegisterPubSubStorageProvider<T>(IDictionary<string, string> properties = null) where T : IStorageProvider
        {
            var registered = cluster.Globals
                .GetAllProviderConfigurations()
                .Any(p => p.Name == "PubSubStore");

            if (registered)
                throw new InvalidOperationException(
                    "PubSub storage provider has been already registered");

            cluster.Globals.RegisterStorageProvider<T>("PubSubStore", properties);
        }

        public new PlaygroundConfigurator StreamProvider<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            base.StreamProvider<T>(name, properties);
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