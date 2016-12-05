using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleankka.Core;
using Orleankka.Utility;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Embedded
{
    using Client;
    using Cluster;

    public class EmbeddedConfigurator
    {
        readonly HashSet<Assembly> assemblies = 
             new HashSet<Assembly>();

        readonly HashSet<ActorInterfaceMapping> interfaces =
             new HashSet<ActorInterfaceMapping>();

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
    
        public EmbeddedConfigurator Run<T>(object properties = null) where T : IBootstrapper
        {
            cluster.Run<T>(properties);
            return this;
        }

        public EmbeddedConfigurator Register<T>(object properties = null) where T : IActorActivator
        {
            cluster.Register<T>(properties);
            return this;
        }

        public EmbeddedConfigurator RegisterInterceptor<T>(object properties = null) where T : IInterceptor
        {
            cluster.RegisterInterceptor<T>(properties);
            return this;
        }

        public EmbeddedConfigurator Register<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            cluster.Register<T>(name, properties);
            client.Register<T>(name, properties);
            return this;
        }

        public EmbeddedConfigurator Register(params Assembly[] assemblies)
        {
            Requires.NotNull(assemblies, nameof(assemblies));

            if (assemblies.Length == 0)
                throw new ArgumentException("Assemblies length should be greater than 0", nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                if (this.assemblies.Contains(assembly))
                    throw new ArgumentException($"Assembly {assembly.FullName} has been already registered");

                this.assemblies.Add(assembly);
            }

            foreach (var type in assemblies.SelectMany(x => x.ActorTypes()))
            {
                var mapping = ActorInterfaceMapping.Of(type);
                if (!interfaces.Add(mapping))
                    throw new ArgumentException($"Actor type '{mapping.Name}' has been already registered");
            }

            client.Register(assemblies, interfaces);
            cluster.Register(assemblies);

            return this;
        }

        public virtual EmbeddedActorSystem Done()
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