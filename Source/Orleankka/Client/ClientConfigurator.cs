using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;
    using Utility;

    public sealed class ClientConfigurator
    {
        readonly Dictionary<string, Assembly> assemblies = 
             new Dictionary<string, Assembly>();
        
        Type serializerType;
        Dictionary<string, string> serializerProperties;
        
        readonly IActorSystemConfigurator configurator;

        internal ClientConfigurator(IActorSystemConfigurator configurator)
        {
            this.configurator = configurator;
            Configuration = new ClientConfiguration();
        }

        public ClientConfiguration Configuration
        {
            get; private set;
        }

        public ClientConfigurator From(ClientConfiguration config)
        {
            Requires.NotNull(config, "config");
            Configuration = config;
            return this;
        }

        public ClientConfigurator Serializer<T>(Dictionary<string, string> properties = null) where T : IMessageSerializer
        {
            serializerType = typeof(T);
            serializerProperties = properties;
            return this;
        }

        public ClientConfigurator Register(params Assembly[] assemblies)
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
            var system = new ClientActorSystem(configurator);
            Configure(system);

            ClientActorSystem.Initialize(Configuration);
            return system;
        }

        internal void Configure(IActorSystem system)
        {
            if (assemblies.Count == 0)
                throw new InvalidOperationException("No actor assemblies were registered. Use Register(assembly) method to register assemblies which contain actor declarations");
            
            configurator.Configure(new ActorSystemConfiguration
            {
                Instance   = system,
                Assemblies = assemblies.Values.ToArray(),
                Serializer = serializerType != null
                                ? Tuple.Create(serializerType, serializerProperties) 
                                : null 
            });
        }
    }

    public static class ClientConfiguratorExtensions
    {
        public static ClientConfigurator Client(this ActorSystemConfigurator configurator)
        {
            Requires.NotNull(configurator, "configurator");
            return new ClientConfigurator(configurator);
        }

        public static ClientConfiguration LoadFromEmbeddedResource<TNamespaceScope>(this ClientConfiguration config, string resourceName)
        {
            return LoadFromEmbeddedResource(config, typeof(TNamespaceScope), resourceName);
        }

        public static ClientConfiguration LoadFromEmbeddedResource(this ClientConfiguration config, Type namespaceScope, string resourceName)
        {
            if (namespaceScope.Namespace == null)
            {
                throw new ArgumentException(
                    "Resource assembly and scope cannot be determined from type '0' since it has no namespace.\nUse overload that takes Assembly and string path to provide full path of the embedded resource");
            }

            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, String.Format("{0}.{1}", namespaceScope.Namespace, resourceName));
        }

        public static ClientConfiguration LoadFromEmbeddedResource(this ClientConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ClientConfiguration();
            result.Load(assembly.LoadEmbeddedResource(fullResourcePath));
            return result;
        }
    }
}