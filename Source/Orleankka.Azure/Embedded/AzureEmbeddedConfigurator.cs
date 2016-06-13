using System;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Embedded
{
    using Core;
    using Client;
    using Cluster;

    public class AzureEmbeddedConfigurator : IActorSystemConfigurator
    {
        readonly AzureClientConfigurator client;
        readonly AzureClusterConfigurator cluster;
        readonly AppDomain domain;

        public AzureEmbeddedConfigurator(AppDomainSetup setup)
        {
            domain  = AppDomain.CreateDomain("EmbeddedOrleans", null, setup ?? AppDomain.CurrentDomain.SetupInformation);
            client  = new AzureClientConfigurator();
            cluster = (AzureClusterConfigurator)domain.CreateInstanceAndUnwrap(
                        GetType().Assembly.FullName, typeof(AzureClusterConfigurator).FullName, false,
                        BindingFlags.NonPublic | BindingFlags.Instance, null,
                        new object[0], null, null);
        }

        public AzureEmbeddedConfigurator From(ClusterConfiguration config)
        {
            cluster.From(config);
            return this;
        }

        public AzureEmbeddedConfigurator From(ClientConfiguration config)
        {
            client.From(config);
            return this;
        }

        public AzureEmbeddedConfigurator Run<T>(object properties = null) where T : IBootstrapper
        {
            cluster.Run<T>(properties);
            return this;
        }

        public AzureEmbeddedConfigurator Hook(ActorSystemConfiguratorHook hook)
        {
            cluster.Hook(hook);
            client.Hook(hook);
            return this;
        }

        public AzureEmbeddedConfigurator Register(params ActorConfiguration[] configs)
        {
            cluster.Register(configs);
            client.Register(configs);
            return this;
        }

        void IActorSystemConfigurator.Hook(ActorSystemConfiguratorHook hook) => this.Hook(hook);
        void IActorSystemConfigurator.Register(ActorConfiguration[] configs) => this.Register(configs);

        public AzureEmbeddedActorSystem Done()
        {
            var clusterSystem = cluster.Done();
            var clientSystem = client.Done();

            return new AzureEmbeddedActorSystem(domain, clientSystem, clusterSystem);
        }    
    }

    public static class AzureEmbeddedConfiguratorExtensions
    {
        public static AzureEmbeddedConfigurator Embedded(this IAzureConfigurator root, AppDomainSetup setup = null)
        {
            return new AzureEmbeddedConfigurator(setup);
        }
    }
}
