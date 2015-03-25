using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core;
    using Utility;

    public class AzureClusterConfigurator : MarshalByRefObject
    {
        readonly AzureConfigurator azure;
        readonly ClusterConfigurator cluster;

        internal AzureClusterConfigurator(AzureConfigurator azure)
        {
            this.azure = azure;
            cluster = new ClusterConfigurator(azure.Configurator);
        }

        public ClusterConfiguration Configuration
        {
            get { return cluster.Configuration; }
        }

        public AzureClusterConfigurator From(ClusterConfiguration config)
        {
            cluster.From(config);
            return this;
        }

        public AzureClusterConfigurator Serializer<T>(Dictionary<string, string> properties = null) where T : IMessageSerializer
        {
            cluster.Serializer<T>(properties);
            return this;
        }

        public AzureClusterConfigurator Activator<T>(Dictionary<string, string> properties = null) where T : IActorActivator
        {
            cluster.Activator<T>(properties);
            return this;
        }

        public AzureClusterConfigurator Run<T>(Dictionary<string, string> properties = null) where T : Bootstrapper
        {
            cluster.Run<T>(properties);
            return this;
        }

        public AzureClusterConfigurator Register(params Assembly[] assemblies)
        {
            cluster.Register(assemblies);
            return this;
        }

        public AzureClusterActorSystem Done()
        {
            var system = new AzureClusterActorSystem(azure.Configurator, Configuration);
            cluster.Configure();

            system.Start();
            return system;
        }
    }

    public static class ClusterConfiguratorExtensions
    {
        public static AzureClusterConfigurator Cluster(this AzureConfigurator configurator)
        {
            Requires.NotNull(configurator, "configurator");
            return new AzureClusterConfigurator(configurator);
        }
    }
}
