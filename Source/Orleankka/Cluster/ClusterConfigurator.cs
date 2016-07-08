using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core.Streams;
    using Utility;

    public sealed class ClusterConfigurator : ActorSystemConfigurator
    {
        readonly HashSet<BootstrapProviderConfiguration> bootstrapProviders =
             new HashSet<BootstrapProviderConfiguration>();

        readonly HashSet<StreamProviderConfiguration> streamProviders =
             new HashSet<StreamProviderConfiguration>();

        internal ClusterConfigurator()
        {
            Configuration = new ClusterConfiguration();
        }

        internal ClusterConfiguration Configuration
        {
            get; private set;
        }

        public ClusterConfigurator From(ClusterConfiguration config)
        {
            Requires.NotNull(config, nameof(config));
            Configuration = config;
            return this;
        }

        public ClusterConfigurator Run<T>(object properties = null) where T : IBootstrapper
        {
            var configuration = new BootstrapProviderConfiguration(typeof(T), properties);

            if (!bootstrapProviders.Add(configuration))
                throw new ArgumentException($"Bootstrapper of the type {typeof(T)} has been already registered");

            return this;
        }

        public ClusterConfigurator Register<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            var configuration = new StreamProviderConfiguration(name, typeof(T), properties);
            if (!streamProviders.Add(configuration))
                throw new ArgumentException($"Stream provider of the type {typeof(T)} has been already registered under '{name}' name");

            return this;
        }

        public ClusterConfigurator Register(params EndpointConfiguration[] configs)
        {
            ((IActorSystemConfigurator)this).Register(configs);
            return this;
        }

        public IActorSystem Done()
        {
            var system = new ClusterActorSystem(this, Configuration);
            Configure();

            system.Start();
            return system;
        }

        internal new void Configure()
        {
            BootstrapStreamSubscriptionHook();

            foreach (var each in streamProviders)
                each.Register(Configuration);

            foreach (var each in bootstrapProviders)
                each.Register(Configuration.Globals);

            base.Configure();
        }

        void BootstrapStreamSubscriptionHook()
        {
            const string id = "stream-subscription-boot";

            var properties = new Dictionary<string, string>();
            properties["providers"] = string.Join(";", streamProviders
                .Where(x => x.IsPersistentStreamProvider())
                .Select(x => x.Name));
              
            Configuration.Globals.RegisterStorageProvider<StreamSubscriptionBootstrapper>(id, properties);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    public static class ClusterConfiguratorExtensions
    {
        public static ClusterConfigurator Cluster(this IActorSystemConfigurator root)
        {
            return new ClusterConfigurator();
        }

        public static ClusterConfiguration LoadFromEmbeddedResource<TNamespaceScope>(this ClusterConfiguration config, string resourceName)
        {
            return LoadFromEmbeddedResource(config, typeof(TNamespaceScope), resourceName);
        }

        public static ClusterConfiguration LoadFromEmbeddedResource(this ClusterConfiguration config, Type namespaceScope, string resourceName)
        {
            if (namespaceScope.Namespace == null)
                throw new ArgumentException("Resource assembly and scope cannot be determined from type '0' since it has no namespace.\nUse overload that takes Assembly and string path to provide full path of the embedded resource");

            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, $"{namespaceScope.Namespace}.{resourceName}");
        }

        public static ClusterConfiguration LoadFromEmbeddedResource(this ClusterConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ClusterConfiguration();
            result.Load(assembly.LoadEmbeddedResource(fullResourcePath));
            return result;
        }

        public static ClusterConfiguration DefaultKeepAliveTimeout(this ClusterConfiguration config, TimeSpan idle)
        {
            Requires.NotNull(config, nameof(config));
            config.Globals.Application.SetDefaultCollectionAgeLimit(idle);
            return config;
        }
    }
}