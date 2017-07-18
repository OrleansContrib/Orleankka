using System;
using System.Collections.Generic;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Embedded
{
    using Client;
    using Cluster;

    public class EmbeddedConfigurator
    {
        readonly ClientConfigurator client;
        readonly ClusterConfigurator cluster;

        public EmbeddedConfigurator()
        {
            client  = new ClientConfigurator();
            cluster = new ClusterConfigurator();
        }

        public EmbeddedConfigurator Cluster(ClusterConfiguration config)
        {
            cluster.From(config);
            return this;
        }

        public EmbeddedConfigurator Client(ClientConfiguration config)
        {
            client.From(config);
            return this;
        }
    
        public EmbeddedConfigurator Bootstrapper<T>(object properties = null) where T : IBootstrapper
        {
            cluster.Bootstrapper<T>(properties);
            return this;
        }

        public EmbeddedConfigurator Activator(IActorActivator activator)
        {
            cluster.Activator(activator);
            return this;
        }

        public EmbeddedConfigurator StreamProvider<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            cluster.StreamProvider<T>(name, properties);
            client.StreamProvider<T>(name, properties);
            return this;
        }

        /// <summary>
        /// Registers global actor invoker (interceptor). This invoker will be used for every actor 
        /// which doesn't specify an individual invoker via <see cref="InvokerAttribute"/> attribute.
        /// </summary>
        /// <param name="global">The invoker.</param>
        public EmbeddedConfigurator ActorInvoker(IActorInvoker global)
        {
            cluster.ActorInvoker(global);
            return this;
        }

        /// <summary>
        /// Registers named actor invoker (interceptor). For this invoker to be used an actor need 
        /// to specify its name via <see cref="InvokerAttribute"/> attribute. 
        /// The invoker is inherited by all subclasses.
        /// </summary>
        /// <param name="name">The name of the invoker</param>
        /// <param name="invoker">The invoker.</param>
        public EmbeddedConfigurator ActorInvoker(string name, IActorInvoker invoker)
        {
            cluster.ActorInvoker(name, invoker);
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
        public static EmbeddedConfigurator Embedded(this IActorSystemConfigurator root, AppDomainSetup setup = null)
        {
            return new EmbeddedConfigurator();
        }
    }
}