using System;
using System.Linq;

using Orleans.Providers.Streams.SimpleMessageStream;
using Orleans.Runtime.Configuration;
using Orleans.Storage;

namespace Orleankka.Playground
{
    using System.Collections.Generic;

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

            cluster.Globals.RegisterStreamProvider<SimpleMessageStreamProvider>("SMS");

            client.RegisterStreamProvider<SimpleMessageStreamProvider>("SMS");
        }

        public PlaygroundConfigurator TweakClient(Action<ClientConfiguration> tweak)
        {
            Requires.NotNull(tweak, nameof(tweak));
            tweak(client);
            return this;
        }

        public PlaygroundConfigurator TweakCluster(Action<ClusterConfiguration> tweak)
        {
            Requires.NotNull(tweak, nameof(tweak));
            tweak(cluster);
            return this;
        }

        public PlaygroundConfigurator UseInMemoryPubSubStore()
        {
            CheckPubSubStorageProviderAlreadyRegistered();
            RegisterPubSubStorageProvider<MemoryStorage>();
            return this;
        }

        public PlaygroundConfigurator UseAzureTablePubSubStore()
        {
            var properties = new Dictionary<string, string>()
            {
                {"DataConnectionString", "UseDevelopmentStorage=true"},
                {"UseJsonFormat", "true"},
                {"TableName", "PubSubData"}
            };

            CheckPubSubStorageProviderAlreadyRegistered();
            RegisterPubSubStorageProvider<AzureTableStorage>(properties);
            return this;
        }

        void CheckPubSubStorageProviderAlreadyRegistered()
        {
            var registered = cluster
                .Globals
                .GetAllProviderConfigurations()
                .Any(p => p.Name == "PubSubStore");

           if (registered)
                throw new InvalidOperationException("PubSub storage provider has been already registered");
        }

        void RegisterPubSubStorageProvider<T>(IDictionary<string, string> properties = null) where T : IStorageProvider
        {
            cluster.Globals.RegisterStorageProvider<T>("PubSubStore", properties);
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