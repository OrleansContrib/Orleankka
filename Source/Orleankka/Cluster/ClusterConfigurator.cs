using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;

using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core;
    using Utility;

    public sealed class ClusterConfigurator : ActorSystemConfigurator
    {
        readonly HashSet<BootstrapProviderConfiguration> bootstrappers =
             new HashSet<BootstrapProviderConfiguration>();

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
            Requires.NotNull(config, "config");
            Configuration = config;
            return this;
        }

        public ClusterConfigurator Serializer<T>(Dictionary<string, string> properties = null) where T : IMessageSerializer
        {
            RegisterSerializer<T>(properties);
            return this;
        }
        
        public ClusterConfigurator Activator<T>(Dictionary<string, string> properties = null) where T : IActorActivator
        {
            RegisterActivator<T>(properties);
            return this;
        }

        public ClusterConfigurator Run<T>(Dictionary<string, string> properties = null) where T : Bootstrapper
        {
            if (!bootstrappers.Add(new BootstrapProviderConfiguration(typeof(T), properties)))
                throw new ArgumentException(
                    string.Format("Bootstrapper of the type {0} has been already registered", typeof(T)));

            return this;
        }

        public ClusterConfigurator Register(params Assembly[] assemblies)
        {
            RegisterAssemblies(assemblies);
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
            foreach (var bootstrapper in bootstrappers)
                bootstrapper.Register(Configuration.Globals);

            base.Configure();
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
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

            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, string.Format("{0}.{1}", namespaceScope.Namespace, resourceName));
        }

        public static ClusterConfiguration LoadFromEmbeddedResource(this ClusterConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ClusterConfiguration();
            result.Load(assembly.LoadEmbeddedResource(fullResourcePath));
            return result;
        }

        public static ClusterConfiguration DefaultKeepAliveTimeout(this ClusterConfiguration config, TimeSpan idle)
        {
            Requires.NotNull(config, "config");
            config.Globals.Application.SetDefaultCollectionAgeLimit(idle);
            return config;
        }
    }
}