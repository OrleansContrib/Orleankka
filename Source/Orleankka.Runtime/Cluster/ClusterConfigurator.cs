using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

using Microsoft.Extensions.DependencyInjection;

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

        readonly HashSet<StreamProviderConfiguration> streamProviders =
             new HashSet<StreamProviderConfiguration>();

        readonly ActorInvocationPipeline pipeline = new ActorInvocationPipeline();

        IActorRefInvoker invoker;
        Action<IServiceCollection> di;

        internal ClusterConfigurator()
        {
            Configuration = new ClusterConfiguration();
        }

        public ClusterConfiguration Configuration { get; set; }

        public ClusterConfigurator From(ClusterConfiguration config)
        {
            Requires.NotNull(config, nameof(config));
            Configuration = config;
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

        public ClusterConfigurator StreamProvider<T>(string name, IDictionary<string, string> properties = null) where T : IStreamProviderImpl
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            var configuration = new StreamProviderConfiguration(name, typeof(T), properties);
            if (!streamProviders.Add(configuration))
                throw new ArgumentException($"Stream provider of the type {typeof(T)} has been already registered under '{name}' name");

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

        public ClusterActorSystem Done()
        {
            Configure();

            return new ClusterActorSystem(Configuration, registry.Assemblies, di, pipeline, invoker);
        }

        void Configure()
        {
            RegisterInterfaces();
            RegisterTypes();
            RegisterAutoruns();
            RegisterStreamProviders();
            RegisterStreamSubscriptions();
            RegisterBootstrappers();
            RegisterBehaviors();
        }

        void RegisterInterfaces() => ActorInterface.Register(registry.Assemblies, registry.Mappings);

        void RegisterTypes() => ActorType.Register(registry.Assemblies, conventions.Count > 0 ? conventions.ToArray() : null);

        void RegisterAutoruns()
        {
            var autoruns = new Dictionary<string, string[]>();

            foreach (var actor in registry.Assemblies.SelectMany(x => x.ActorTypes()))
            {
                var ids = AutorunAttribute.From(actor);
                if (ids.Length > 0)
                    autoruns.Add(ActorTypeName.Of(actor), ids);
            }

            Bootstrapper<AutorunBootstrapper>(autoruns);
        }

        void RegisterStreamProviders()
        {
            foreach (var each in streamProviders)
                each.Register(Configuration);
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
                StreamSubscriptionMatcher.Register(actor.Name, actor.Subscriptions());

            const string id = "stream-subscription-boot";

            var properties = new Dictionary<string, string>();
            properties["providers"] = string.Join(";", streamProviders
                .Where(x => x.IsPersistentStreamProvider())
                .Select(x => x.Name));

            Configuration.Globals.RegisterStorageProvider<StreamSubscriptionBootstrapper>(id, properties);
        }

        [UsedImplicitly]
        class AutorunBootstrapper : Bootstrapper<Dictionary<string, string[]>>
        {
            protected override Task Run(IActorSystem system, Dictionary<string, string[]> properties) =>
                Task.WhenAll(properties.SelectMany(x => Autorun(system, x.Key, x.Value)));

            static IEnumerable<Task> Autorun(IActorSystem system, string type, IEnumerable<string> ids) =>
                ids.Select(id => system.ActorOf(type, id).Autorun());
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