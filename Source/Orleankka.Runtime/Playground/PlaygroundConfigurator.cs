using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

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

            Register<SimpleMessageStreamProvider>("sms", new Dictionary<string, string> {{"FireAndForgetDelivery", "false"}});
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
            RegisterPubSubStorageProvider<MemoryStorage>();
            return this;
        }

        internal void RegisterPubSubStorageProvider<T>(IDictionary<string, string> properties = null) where T : IStorageProvider
        {
            var registered = cluster.Globals
                .GetAllProviderConfigurations()
                .Any(p => p.Name == "PubSubStore");

            if (registered)
                throw new InvalidOperationException(
                    "PubSub storage provider has been already registered");

            cluster.Globals.RegisterStorageProvider<T>("PubSubStore", properties);
        }

        public new PlaygroundConfigurator Register<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            base.Register<T>(name, properties);
            return this;
        }

        public override EmbeddedActorSystem Done()
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