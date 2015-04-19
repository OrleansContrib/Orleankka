using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core;

    public class AzureClusterConfigurator : MarshalByRefObject
    {
        readonly ClusterConfigurator cluster;

        internal AzureClusterConfigurator()
        {
            cluster = new ClusterConfigurator();
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
            var system = new AzureClusterActorSystem(cluster);
            cluster.Configure();
            
            system.Start();
            return system;
        }
    }

    public static class ClusterConfiguratorExtensions
    {
        public static AzureClusterConfigurator Cluster(this IAzureConfigurator root)
        {
            return new AzureClusterConfigurator();
        }
    }
}
