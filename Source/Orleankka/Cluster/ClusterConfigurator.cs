using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

using Orleans.Providers;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

namespace Orleankka.Cluster
{
    using Core;
    using Utility;

    public sealed class ClusterConfigurator : MarshalByRefObject
    {
        readonly HashSet<BootstrapProviderConfiguration> bootstrappers =
             new HashSet<BootstrapProviderConfiguration>();
        
        readonly Dictionary<string, Assembly> assemblies = 
             new Dictionary<string, Assembly>();
        
        Type serializerType;
        Dictionary<string, string> serializerProperties;

        Type activatorType;
        Dictionary<string, string> activatorProperties;
        
        readonly IActorSystemConfigurator configurator;

        ClusterConfigurator() 
            : this(new ActorSystemConfigurator())
        {}

        internal ClusterConfigurator(IActorSystemConfigurator configurator)
        {
            this.configurator = configurator;
            Configuration = new ClusterConfiguration();
        }

        public ClusterConfiguration Configuration
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
            serializerType = typeof(T);
            serializerProperties = properties;
            return this;
        }
        
        public ClusterConfigurator Activator<T>(Dictionary<string, string> properties = null) where T : IActorActivator
        {
            activatorType = typeof(T);
            activatorProperties = properties;
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
            Requires.NotNull(assemblies, "assemblies");

            if (assemblies.Length == 0)
                throw new ArgumentException("Assemblies length should be greater than 0", "assemblies");

            foreach (var assembly in assemblies)
            {
                if (this.assemblies.ContainsKey(assembly.FullName))
                    throw new ArgumentException(
                        string.Format("Assembly {0} has been already registered", assembly.FullName));

                this.assemblies.Add(assembly.FullName, assembly);
            }

            return this;
        }

        public IActorSystem Done()
        {
            if (assemblies.Count == 0)
                throw new InvalidOperationException("No actor assemblies were registered. Use Register(assembly) method to register assemblies which contain actor declarations");

            RegisterBootstrappers();

            var host = new SiloHost(Dns.GetHostName(), Configuration);
            var system = new ClusterActorSystem(configurator, host);
            
            configurator.Configure(new ActorSystemConfiguration
            {
                Instance   = system,
                Assemblies = assemblies.Values.ToArray(),
                Serializer = serializerType != null 
                                ? Tuple.Create(serializerType, serializerProperties) 
                                : null,
                Activator  = activatorType != null 
                                ? Tuple.Create(activatorType, activatorProperties) 
                                : null 
            });

            host.LoadOrleansConfig();
            host.InitializeOrleansSilo();
            host.StartOrleansSilo();

            return system;
        }

        void RegisterBootstrappers()
        {
            var category = Configuration.Globals.ProviderConfigurations.Find("Bootstrap");

            if (category == null)
            {
                category = new ProviderCategoryConfiguration
                {
                    Name = "Bootstrap",
                    Providers = new Dictionary<string, IProviderConfiguration>()
                };

                Configuration.Globals.ProviderConfigurations.Add("Bootstrap", category);
            }

            foreach (var bootstrapper in bootstrappers)
                bootstrapper.Register(category);
        }
    }

    public static class ClusterConfiguratorExtensions
    {
        public static ClusterConfigurator Cluster(this ActorSystemConfigurator configurator)
        {
            Requires.NotNull(configurator, "configurator");
            return new ClusterConfigurator(configurator);
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
    }
}