using System;
using System.Collections.Generic;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;

    using Microsoft.Extensions.DependencyInjection;

    using Utility;

    public sealed class ClientConfigurator
    {
        readonly ActorInterfaceRegistry registry =
             new ActorInterfaceRegistry();

        readonly HashSet<StreamProviderConfiguration> streamProviders =
             new HashSet<StreamProviderConfiguration>();

        Action<IServiceCollection> configure;

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

        public ClientConfigurator StreamProvider<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProvider
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            var configuration = new StreamProviderConfiguration(name, typeof(T), properties);
            if (!streamProviders.Add(configuration))
                throw new ArgumentException($"Stream provider of the type {typeof(T)} has been already registered under '{name}' name");

            return this;
        }

        public ClientConfigurator Services(Action<IServiceCollection> configure)
        {
            Requires.NotNull(configure, nameof(configure));

            if (this.configure != null)
                throw new InvalidOperationException("Services configurator has been already set");

            this.configure = configure;

            return this;
        }

        public ClientConfigurator Assemblies(params Assembly[] assemblies)
        {
            registry.Register(assemblies, a => a.ActorInterfaces());

            return this;
        }

        internal ClientConfigurator ActorTypes(params string[] types)
        {
            Requires.NotNull(types, nameof(types));

            if (types.Length == 0)
                throw new ArgumentException("types array is empty", nameof(types));

            foreach (var type in types)
            {
                var mapping = ActorInterfaceMapping.Of(type);
                if (registry.IsRegistered(mapping.TypeName))
                    throw new ArgumentException($"Actor type '{type}' has been already registered");
            }

            return this;
        }

        public ClientActorSystem Done()
        {
            RegisterStreamProviders();
            RegisterActorInterfaces();

            return new ClientActorSystem(Configuration, configure);
        }

        void RegisterStreamProviders()
        {
            foreach (var each in streamProviders)
                each.Register(Configuration);
        }

        void RegisterActorInterfaces() => ActorInterface.Register(registry.Assemblies, registry.Mappings);
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