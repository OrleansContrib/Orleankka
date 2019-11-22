using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans.Streams;
using Orleans.Runtime.Configuration;
using Orleans.Hosting;
using Orleans.Configuration;
using Orleans.Runtime;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Orleankka.Cluster
{
    using Core;
    using Core.Streams;
    using Behaviors;
    using Utility;

    public sealed class ClusterConfigurator
    {
        readonly ActorInterfaceRegistry registry =
             new ActorInterfaceRegistry();

        readonly HashSet<string> conventions = new HashSet<string>();

        readonly ActorMiddlewarePipeline pipeline = 
             new ActorMiddlewarePipeline();
        
        IActorRefMiddleware middleware;
        Action<IServiceCollection> di;
        Action<ISiloHostBuilder> builder;
        string[] persistentStreamProviders = new string[0];
        
        internal ClusterConfigurator()
        {
            Configuration = ClusterConfiguration.LocalhostPrimarySilo();
        }

        public ClusterConfiguration Configuration { get; set; }

        public ClusterConfigurator From(ClusterConfiguration config)
        {
            Requires.NotNull(config, nameof(config));
            Configuration = config;
            return this;
        }
        
        public ClusterConfigurator Builder(Action<ISiloHostBuilder> builder)
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

        public ClusterConfigurator Assemblies(params Assembly[] assemblies)
        {
            registry.Register(assemblies, a => a.ActorTypes());

            return this;
        }

        public ClusterConfigurator Services(Action<IServiceCollection> configure)
        {
            Requires.NotNull(configure, nameof(configure));

            if (di != null)
                throw new InvalidOperationException("Services configurator has been already set");

            di = configure;
            return this;
        }

        /// <summary>
        /// Registers global actor middleware (interceptor). This middleware will be used for every actor 
        /// which doesn't specify an individual middleware via call to <see cref="ActorMiddleware(Type,IActorMiddleware) "/>.
        /// </summary>
        /// <param name="global">The middleware.</param>
        public ClusterConfigurator ActorMiddleware(IActorMiddleware global)
        {
            pipeline.Register(global);
            return this;
        }

        /// <summary>
        /// Registers type-based actor middleware (interceptor). 
        /// The middleware is inherited by all subclasses.
        /// </summary>
        /// <param name="type">The actor type (could be the base class)</param>
        /// <param name="middleware">The middleware.</param>
        public ClusterConfigurator ActorMiddleware(Type type, IActorMiddleware middleware)
        {
            pipeline.Register(type, middleware);
            return this;
        }

        /// <summary>
        /// Registers global <see cref="ActorRef"/> middleware (interceptor)
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        public ClusterConfigurator ActorRefMiddleware(IActorRefMiddleware middleware)
        {
            Requires.NotNull(middleware, nameof(middleware));

            if (this.middleware != null)
                throw new InvalidOperationException("ActorRef middleware has been already registered");

            this.middleware = middleware;
            return this;
        }

        public ClusterConfigurator HandlerNamingConventions(params string[] conventions)
        {
            Requires.NotNull(conventions, nameof(conventions));

            if (conventions.Length == 0)
                throw new ArgumentException("conventions are empty", nameof(conventions));

            Array.ForEach(conventions, x => this.conventions.Add(x));

            return this;
        }

        public ClusterConfigurator UseSimpleMessageStreamProvider(string name, Action<OptionsBuilder<SimpleMessageStreamProviderOptions>> configureOptions = null)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            Builder(b => b.ConfigureServices(services =>
            {
                configureOptions?.Invoke(OptionsServiceCollectionExtensions.AddOptions<SimpleMessageStreamProviderOptions>(services, name));
                services.ConfigureNamedOptionForLogging<SimpleMessageStreamProviderOptions>(name);
                services.AddSingletonNamedService<IStreamProvider>(name, (s, n) => new StreamSubscriptionMatcher(s, n));
            }));
            
            return this;
        }

        public ClusterConfigurator RegisterPersistentStreamProviders(params string[] names)
        {
            Requires.NotNull(names, nameof(names));
            persistentStreamProviders = names;

            return this;
        }

        public ClusterActorSystem Done()
        {
            var generatedAssemblies = Configure();
            
            return new ClusterActorSystem(Configuration, builder, persistentStreamProviders, registry.Assemblies, generatedAssemblies, di, pipeline, middleware);
        }

        Assembly[] Configure()
        {
            var generatedAssemblies = RegisterInterfaces().ToList();
            generatedAssemblies.AddRange(RegisterTypes());

            RegisterStreamSubscriptions();
            RegisterBehaviors();

            return generatedAssemblies.ToArray();
        }

        Assembly[] RegisterInterfaces() => ActorInterface.Register(registry.Assemblies, registry.Mappings);

        Assembly[] RegisterTypes() => ActorType.Register(registry.Assemblies, conventions.Count > 0 ? conventions.ToArray() : null);

        void RegisterBehaviors()
        {
            foreach (var actor in registry.Assemblies.SelectMany(x => x.ActorTypes()))
                ActorBehavior.Register(actor);
        }

        void RegisterStreamSubscriptions()
        {
            foreach (var actor in ActorType.Registered())
                StreamSubscriptionMatcher.Register(actor.FullName, actor.Subscriptions());
        }

        public ClusterConfigurator UseInMemoryPubSubStore() => UseInMemoryGrainStore("PubSubStore");

        public ClusterConfigurator UseInMemoryGrainStore(string name = "MemoryStore") => 
            Builder(sb => sb.AddMemoryGrainStorage(name));
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
    }
}