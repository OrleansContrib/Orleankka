using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Embedded
{
    using Client;
    using Cluster;

    public class EmbeddedConfigurator : IExtensibleActorSystemConfigurator
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
    
        public EmbeddedConfigurator Run<T>(object properties = null) where T : IBootstrapper
        {
            cluster.Run<T>(properties);
            return this;
        }

        public EmbeddedConfigurator Register<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            cluster.Register<T>(name, properties);
            client.Register<T>(name, properties);
            return this;
        }

        public EmbeddedConfigurator Invoke(Action<IActorSystemConfigurator> configure)
        {
            if (configure.Method.IsStatic || !configure.Method.IsPublic)
                throw new ArgumentException("'configure' should be a public non-static method");

            ((IExtensibleActorSystemConfigurator)this).Extend<StaticMethodConfiguratorExtension>(
                ext => ext.Method(configure.Method));

            return this;
        }

        void IExtensibleActorSystemConfigurator.Extend<T>(Action<T> configure)
        {
            configure(client.Add<T>());
            configure(cluster.Add<T>());
        }

        public virtual EmbeddedActorSystem Done()
        {
            var clusterSystem = cluster.Done();
            var clientSystem = client.Done();

            // connect automatically
            clientSystem.Connect();

            return new EmbeddedActorSystem(domain, clientSystem, clusterSystem);
        }

        public class StaticMethodConfiguratorExtension : ActorSystemConfiguratorExtension
        {
            Action<IActorSystemConfigurator> configure;
             
            public void Method(MethodInfo method)
            {
                Debug.Assert(method.DeclaringType != null);
                var target = Activator.CreateInstance(method.DeclaringType);
                configure = (Action<IActorSystemConfigurator>) method.CreateDelegate(typeof(Action<IActorSystemConfigurator>), target);
            }

            protected internal override void Configure(IActorSystemConfigurator configurator) => 
                configure(configurator);
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