using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;
    using Utility;

    public sealed class ClientConfigurator : IDisposable
    {
        readonly HashSet<Assembly> assemblies = 
             new HashSet<Assembly>();

        readonly HashSet<ActorInterfaceMapping> interfaces =
             new HashSet<ActorInterfaceMapping>();

        readonly HashSet<StreamProviderConfiguration> streamProviders =
             new HashSet<StreamProviderConfiguration>();

        internal ClientConfigurator()
        {
            Configuration = new ClientConfiguration();
        }

        ClientConfiguration Configuration { get; set; }

        public ClientConfigurator From(ClientConfiguration config)
        {
            Requires.NotNull(config, nameof(config));
            Configuration = config;
            return this;
        }

        public ClientConfigurator Register<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProvider
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            var configuration = new StreamProviderConfiguration(name, typeof(T), properties);
            if (!streamProviders.Add(configuration))
                throw new ArgumentException($"Stream provider of the type {typeof(T)} has been already registered under '{name}' name");

            return this;
        }

        public ClientConfigurator Register(params Assembly[] assemblies)
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

            foreach (var @interface in assemblies.SelectMany(x => x.ActorInterfaces()))
            {
                var mapping = ActorInterfaceMapping.Of(@interface);
                if (!interfaces.Add(mapping))
                    throw new ArgumentException($"Actor type '{mapping.Name}' has been already registered");
            }
            
            return this;
        }

        internal void Register(IEnumerable<Assembly> assemblies, IEnumerable<ActorInterfaceMapping> mappings)
        {
            foreach (var assembly in assemblies)
                this.assemblies.Add(assembly);

            foreach (var mapping in mappings)
                interfaces.Add(mapping);
        }

        public ClientConfigurator Register(params string[] types)
        {
            Requires.NotNull(types, nameof(types));

            if (types.Length == 0)
                throw new ArgumentException("types array is empty", nameof(types));

            foreach (var type in types)
            {
                if (!interfaces.Add(ActorInterfaceMapping.Of(type)))
                    throw new ArgumentException($"Actor type '{type}' has been already registered");
            }

            return this;
        }

        public ClientActorSystem Done()
        {
            RegisterStreamProviders();
            RegisterActorInterfaces();

            return new ClientActorSystem(this, Configuration);
        }

        void RegisterStreamProviders()
        {
            foreach (var each in streamProviders)
                each.Register(Configuration);
        }

        void RegisterActorInterfaces()
        {
            ActorInterface.Register(assemblies, interfaces);
        }

        public void Dispose() => ActorInterface.Reset();
    }

    public static class ClientConfiguratorExtensions
    {
        public static ClientConfigurator Client(this IActorSystemConfigurator _)
        {
            return new ClientConfigurator();
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

            return LoadFromEmbeddedResource(config, namespaceScope.Assembly, $"{namespaceScope.Namespace}.{resourceName}");
        }

        public static ClientConfiguration LoadFromEmbeddedResource(this ClientConfiguration config, Assembly assembly, string fullResourcePath)
        {
            var result = new ClientConfiguration();
            result.Load(assembly.LoadEmbeddedResource(fullResourcePath));
            return result;
        }
    }
}