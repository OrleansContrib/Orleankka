using System;
using System.Collections.Generic;
using System.Reflection;

using Orleans.Streams;

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

        public EmbeddedConfigurator StreamProvider<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            cluster.StreamProvider<T>(name, properties);
            client.StreamProvider<T>(name, properties);
            return this;
        }

        public EmbeddedConfigurator Assemblies(params Assembly[] assemblies)
        {
            cluster.Assemblies(assemblies);
            client.Assemblies(assemblies);

            return this;
        }

        public virtual EmbeddedActorSystem Done()
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