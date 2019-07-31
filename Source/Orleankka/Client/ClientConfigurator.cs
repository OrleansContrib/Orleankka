using System;
using System.Reflection;

using Orleans.Runtime.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Orleankka.Client
{
    using Core;
    using Orleans;
    using Orleans.Configuration;
    using Orleans.Hosting;

    using Utility;

    public sealed class ClientConfigurator
    {
        readonly ActorInterfaceRegistry registry =
             new ActorInterfaceRegistry();

        Action<IServiceCollection> di;
        IActorRefInvoker invoker;
        Action<IClientBuilder> builder;

        internal ClientConfigurator()
        {
            Configuration = ClientConfiguration.LocalhostSilo();
        }

        public ClientConfiguration Configuration { get; set; }

        public ClientConfigurator From(ClientConfiguration config)
        {
            Requires.NotNull(config, nameof(config));
            Configuration = config;
            return this;
        }

        public ClientConfigurator Builder(Action<IClientBuilder> builder)
        {
            Requires.NotNull(builder, nameof(builder));

            var current = this.builder;
            this.builder = b =>
            {
                current?.Invoke(b);
                builder(b);
            };

            return this;
        }
        
        public ClientConfigurator Services(Action<IServiceCollection> configure)
        {
            Requires.NotNull(configure, nameof(configure));

            if (di != null)
                throw new InvalidOperationException("Services configurator has been already set");

            di = configure;
            return this;
        }

        /// <summary>
        /// Registers global <see cref="ActorRef"/> invoker (interceptor)
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        public ClientConfigurator ActorRefInvoker(IActorRefInvoker invoker)
        {
            Requires.NotNull(invoker, nameof(invoker));

            if (this.invoker != null)
                throw new InvalidOperationException("ActorRef invoker has been already registered");

            this.invoker = invoker;
            return this;
        }

        public ClientConfigurator Assemblies(params Assembly[] assemblies)
        {
            registry.Register(assemblies, a => a.ActorInterfaces());

            return this;
        }

        public ClientConfigurator ActorTypes(params string[] types)
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

        public ClientConfigurator UseSimpleMessageStreamProvider(string name, Action<OptionsBuilder<SimpleMessageStreamProviderOptions>> configureOptions = null)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            Builder(b => b.AddSimpleMessageStreamProvider(name, configureOptions));

            return this;
        }

        public ClientActorSystem Done()
        {
            var generatedAssemblies = RegisterActorInterfaces();

            return new ClientActorSystem(Configuration, builder, registry.Assemblies, generatedAssemblies, di, invoker);
        }

        Assembly[] RegisterActorInterfaces() => ActorInterface.Register(registry.Assemblies, registry.Mappings);
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