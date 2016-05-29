using System;
using System.Collections.Generic;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core;
    using Utility;

    public class AzureClusterConfigurator : MarshalByRefObject
    {
        readonly ClusterConfigurator cluster;

        string deploymentId;
        string connectionString;

        internal AzureClusterConfigurator()
        {
            cluster = new ClusterConfigurator();
        }

        public AzureClusterConfigurator From(ClusterConfiguration config)
        {
            cluster.From(config);
            return this;
        }

        public AzureClusterConfigurator Activator<T>(object properties = null) where T : IActorActivator
        {
            cluster.Activator<T>(properties);
            return this;
        }

        public AzureClusterConfigurator Run<T>(object properties = null) where T : IBootstrapper
        {
            cluster.Run<T>(properties);
            return this;
        }

        public AzureClusterConfigurator Register<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            cluster.Register<T>(name, properties);
            return this;
        }

        public AzureClusterConfigurator Register(params Assembly[] assemblies)
        {
            cluster.Register(assemblies);
            return this;
        }

        public AzureClusterActorSystem Done()
        {
            var system = new AzureClusterActorSystem(cluster, deploymentId, connectionString);
            cluster.Configure();

            system.Start();
            return system;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public AzureClusterConfigurator DeploymentId(string deploymentId)
        {
            Requires.NotNullOrWhitespace(deploymentId, nameof(deploymentId));
            this.deploymentId = deploymentId;
            return this;
        }

        public AzureClusterConfigurator ConnectionString(string connectionString)
        {
            Requires.NotNullOrWhitespace(connectionString, nameof(connectionString));
            this.connectionString = connectionString;
            return this;
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