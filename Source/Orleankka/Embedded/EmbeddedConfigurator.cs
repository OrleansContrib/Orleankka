using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Embedded
{
    using Core;
    using Client;
    using Cluster;

    public class EmbeddedConfigurator
    {
        readonly ClientConfigurator client;
        readonly ClusterConfigurator cluster;
        readonly AppDomain domain;

        public EmbeddedConfigurator(AppDomainSetup setup)
        {
            domain  = AppDomain.CreateDomain("EmbeddedOrleans", null, setup ?? AppDomain.CurrentDomain.SetupInformation);
            client  = new ClientConfigurator();
            cluster = (ClusterConfigurator)domain.CreateInstanceAndUnwrap(
                        GetType().Assembly.FullName, typeof(ClusterConfigurator).FullName, false,
                        BindingFlags.NonPublic | BindingFlags.Instance , null,
                        new object[0], null, null);
        }

        public EmbeddedConfigurator From(ClusterConfiguration config)
        {
            cluster.From(config);
            return this;
        }

        public EmbeddedConfigurator From(ClientConfiguration config)
        {
            client.From(config);
            return this;
        }
    
        public EmbeddedConfigurator Serializer<T>(Dictionary<string, string> properties = null) where T : IMessageSerializer
        {
            client.Serializer<T>(properties);
            cluster.Serializer<T>(properties);
            return this;
        }

        public EmbeddedConfigurator Activator<T>(Dictionary<string, string> properties = null) where T : IActorActivator
        {
            cluster.Activator<T>(properties);
            return this;
        }

        public EmbeddedConfigurator Run<T>(Dictionary<string, string> properties = null) where T : Bootstrapper
        {
            cluster.Run<T>(properties);
            return this;
        }

        public EmbeddedConfigurator Register(params Assembly[] assemblies)
        {
            client.Register(assemblies);
            cluster.Register(assemblies);
            return this;
        }

        public virtual IActorSystem Done()
        {
            var clusterSystem = cluster.Done();
            var clientSystem = client.Done();

            return new EmbeddedActorSystem(domain, clientSystem, clusterSystem);
        }
    }

    public static class EmbeddedConfiguratorExtensions
    {
        public static EmbeddedConfigurator Embedded(this IActorSystemConfigurator root, AppDomainSetup setup = null)
        {
            return new EmbeddedConfigurator(setup);
        }
    }
}