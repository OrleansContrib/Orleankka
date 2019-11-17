using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
    using Annotations;

    public sealed class ClusterConfigurator
    {
        readonly ActorInterfaceRegistry registry =
             new ActorInterfaceRegistry();

        readonly HashSet<string> conventions = new HashSet<string>();

        readonly HashSet<BootstrapProviderConfiguration> bootstrapProviders =
             new HashSet<BootstrapProviderConfiguration>();

        readonly ActorInvocationPipeline pipeline = new ActorInvocationPipeline();
        
        IActorRefInvoker invoker;
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

        public ClusterConfigurator Bootstrapper<T>(object properties = null) where T : IBootstrapper
        {
            var configuration = new BootstrapProviderConfiguration(typeof(T), properties);

            if (!bootstrapProviders.Add(configuration))
                throw new ArgumentException($"Bootstrapper of the type {typeof(T)} has been already registered");

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
        /// Registers global actor invoker (interceptor). This invoker will be used for every actor 
        /// which doesn't specify an individual invoker via <see cref="InvokerAttribute"/> attribute.
        /// </summary>
        /// <param name="global">The invoker.</param>
        public ClusterConfigurator ActorInvoker(IActorInvoker global)
        {
            pipeline.Register(global);
            return this;
        }

        /// <summary>
        /// Registers named actor invoker (interceptor). For this invoker to be used an actor need 
        /// to specify its name via <see cref="InvokerAttribute"/> attribute. 
        /// The invoker is inherited by all subclasses.
        /// </summary>
        /// <param name="name">The name of the invoker</param>
        /// <param name="invoker">The invoker.</param>
        public ClusterConfigurator ActorInvoker(string name, IActorInvoker invoker)
        {
            pipeline.Register(name, invoker);
            return this;
        }

        /// <summary>
        /// Registers global <see cref="ActorRef"/> invoker (interceptor)
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        public ClusterConfigurator ActorRefInvoker(IActorRefInvoker invoker)
        {
            Requires.NotNull(invoker, nameof(invoker));

            if (this.invoker != null)
                throw new InvalidOperationException("ActorRef invoker has been already registered");

            this.invoker = invoker;
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
            
            return new ClusterActorSystem(Configuration, builder, persistentStreamProviders, registry.Assemblies, generatedAssemblies, di, pipeline, invoker);
        }

        Assembly[] Configure()
        {
            var generatedAssemblies = RegisterInterfaces().ToList();
            generatedAssemblies.AddRange(RegisterTypes());

            RegisterAutoruns();
            RegisterStreamSubscriptions();
            RegisterBootstrappers();
            RegisterBehaviors();

            return generatedAssemblies.ToArray();
        }

        Assembly[] RegisterInterfaces() => ActorInterface.Register(registry.Assemblies, registry.Mappings);

        Assembly[] RegisterTypes() => ActorType.Register(registry.Assemblies, conventions.Count > 0 ? conventions.ToArray() : null);

        void RegisterAutoruns()
        {
            var autoruns = new Dictionary<string, string[]>();

            foreach (var actor in registry.Assemblies.SelectMany(x => x.ActorTypes()))
            {
                var ids = AutorunAttribute.From(actor);
                if (ids.Length > 0)
                    autoruns.Add(ActorCustomInterface.Of(actor).AssemblyQualifiedName, ids);
            }

            Bootstrapper<AutorunBootstrapper>(autoruns);
        }

        void RegisterBootstrappers()
        {
            foreach (var each in bootstrapProviders)
                each.Register(Configuration.Globals);
        }

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

        [UsedImplicitly]
        class AutorunBootstrapper : Bootstrapper<Dictionary<string, string[]>>
        {
            protected override Task Run(IActorSystem system, Dictionary<string, string[]> properties) =>
                Task.WhenAll(properties.SelectMany(x => Autorun(system, x.Key, x.Value)));

            static IEnumerable<Task> Autorun(IActorSystem system, string type, IEnumerable<string> ids) =>
                ids.Select(id => system.ActorOf(ActorPath.For(Type.GetType(type), id)).Autorun());
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

        public static ClusterConfiguration DefaultKeepAliveTimeout(this ClusterConfiguration config, TimeSpan idle)
        {
            Requires.NotNull(config, nameof(config));
            config.Globals.Application.SetDefaultCollectionAgeLimit(idle);
            return config;
        }
    }
}