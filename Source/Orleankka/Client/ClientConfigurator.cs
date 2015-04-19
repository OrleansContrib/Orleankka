using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;
    using Utility;

    public sealed class ClientConfigurator : ActorSystemConfigurator
    {
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
            Requires.NotNull(config, "config");
            Configuration = config;
            return this;
        }

        public ClientConfigurator Serializer<T>(Dictionary<string, string> properties = null) where T : IMessageSerializer
        {
            RegisterSerializer<T>(properties);
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
            
            ClientActorSystem.Initialize(Configuration);
            return system;
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