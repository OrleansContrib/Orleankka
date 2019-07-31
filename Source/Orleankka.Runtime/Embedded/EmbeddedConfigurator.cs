using System;
using System.Reflection;

using Microsoft.Extensions.Options;

using Orleans.Configuration;

namespace Orleankka.Embedded
{
    using Client;
    using Cluster;
    using Utility;

    public class EmbeddedConfigurator
    {
        readonly ClientConfigurator client;
        readonly ClusterConfigurator cluster;

        public EmbeddedConfigurator()
        {
            client  = new ClientConfigurator();
            cluster = new ClusterConfigurator();
        }

        public EmbeddedConfigurator Client(Action<ClientConfigurator> configure)
        {
            Requires.NotNull(configure, nameof(configure));
            configure(client);
            return this;
        }

        public EmbeddedConfigurator Cluster(Action<ClusterConfigurator> configure)
        {
            Requires.NotNull(configure, nameof(configure));
            configure(cluster);
            return this;
        }

        public EmbeddedConfigurator Assemblies(params Assembly[] assemblies)
        {
            cluster.Assemblies(assemblies);
            client.Assemblies(assemblies);

            return this;
        }

        public EmbeddedConfigurator UseSimpleMessageStreamProvider(string name, Action<OptionsBuilder<SimpleMessageStreamProviderOptions>> configureOptions = null)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            cluster.UseSimpleMessageStreamProvider(name, configureOptions);
            client.UseSimpleMessageStreamProvider(name, configureOptions);

            return this;
        }

        public EmbeddedActorSystem Done()
        {
            var clusterSystem = cluster.Done();
            var clientSystem = client.Done();

            return new EmbeddedActorSystem(clientSystem, clusterSystem);
        }
    }

    public static class EmbeddedConfiguratorExtensions
    {
        public static EmbeddedConfigurator Embedded(this IActorSystemConfigurator root)
        {
            return new EmbeddedConfigurator();
        }
    }
}