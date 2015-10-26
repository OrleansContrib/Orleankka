using System;
using System.Collections.Generic;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;
    using Core.Streams;
    using Utility;

    public sealed class ClientConfigurator : ActorSystemConfigurator
    {
        readonly HashSet<StreamProviderConfiguration> streamProviders =
             new HashSet<StreamProviderConfiguration>();

        internal ClientConfigurator()
        {
            Configuration = new ClientConfiguration();
        }

        internal ClientConfiguration Configuration
        {
            get; private set;
        }

        public ClientConfigurator From(ClientConfiguration config)
        {
            Requires.NotNull(config, nameof(config));
            Configuration = config;
            return this;
        }

        public ClientConfigurator Serializer<T>(object properties = null) where T : IMessageSerializer
        {
            RegisterSerializer<T>(properties);
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
            RegisterAssemblies(assemblies);
            return this;
        }

        public IActorSystem Done()
        {
            var system = new ClientActorSystem(this);
            Configure();
            
            system.Initialize(Configuration);
            return system;
        }

        internal new void Configure()
        {
            foreach (var each in streamProviders)
                each.Register(Configuration);

            base.Configure();
        }
    }

    public static class ClientConfiguratorExtensions
    {
        public static ClientConfigurator Client(this IActorSystemConfigurator root)
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