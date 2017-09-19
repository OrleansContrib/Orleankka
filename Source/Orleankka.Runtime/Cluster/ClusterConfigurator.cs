using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleans.Streams;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core;
    using Core.Streams;
    using Behaviors;
    using Utility;
    using Annotations;

    public sealed class ClusterConfigurator : MarshalByRefObject
    {
        readonly HashSet<ActorInterfaceMapping> interfaces =
            new HashSet<ActorInterfaceMapping>();

        readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();
        readonly HashSet<string> conventions = new HashSet<string>();

        readonly HashSet<BootstrapProviderConfiguration> bootstrapProviders =
            new HashSet<BootstrapProviderConfiguration>();

        readonly HashSet<StreamProviderConfiguration> streamProviders =
            new HashSet<StreamProviderConfiguration>();

        Tuple<Type, object> activator;
        Tuple<Type, object> interceptor;

        internal ClusterConfigurator()
        {
            Configuration = new ClusterConfiguration();
        }

        ClusterConfiguration Configuration { get; set; }

        public ClusterConfigurator From(ClusterConfiguration config)
        {
            Requires.NotNull(config, nameof(config));
            Configuration = config;
            return this;
        }

        public ClusterConfigurator Assemblies(params Assembly[] assemblies)
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

            foreach (var type in assemblies.SelectMany(x => x.ActorTypes()))
            {
                var mapping = ActorInterfaceMapping.Of(type);
                if (!interfaces.Add(mapping))
                    throw new ArgumentException($"Actor type '{mapping.Name}' has been already registered");
            }

            return this;
        }

        public ClusterConfigurator Bootstrapper<T>(object properties = null) where T : IBootstrapper
        {
            var configuration = new BootstrapProviderConfiguration(typeof(T), properties);

            if (!bootstrapProviders.Add(configuration))
                throw new ArgumentException($"Bootstrapper of the type {typeof(T)} has been already registered");

            return this;
        }

        public ClusterConfigurator Activator<T>(object properties = null) where T : IActorActivator
        {
            if (activator != null)
                throw new InvalidOperationException("Activator has been already registered");

            activator = Tuple.Create(typeof(T), properties);

            return this;
        }

        public ClusterConfigurator Interceptor<T>(object properties = null) where T : IInterceptor
        {
            if (interceptor != null)
                throw new InvalidOperationException("Interceptor has been already registered");

            interceptor = Tuple.Create(typeof(T), properties);

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

        public ClusterConfigurator HandlerNamingConventions(params string[] conventions)
        {
            Requires.NotNull(conventions, nameof(conventions));
            Array.ForEach(conventions, x => this.conventions.Add(x));

            return this;
        }

        public ClusterActorSystem Done()
        {
            Configure();

            return new ClusterActorSystem(Configuration);
        }

        void Configure()
        {
            ConfigureConventions();
            ConfigureActivator();
            ConfigureInterceptor();

            RegisterInterfaces();
            RegisterTypes();
            RegisterAutoruns();
            RegisterStreamProviders();
            RegisterStorageProviders();
            RegisterStreamSubscriptions();
            RegisterBootstrappers();
            RegisterBehaviors();
        }

        void ConfigureConventions()
        {
            ActorType.Conventions = conventions.Count > 0
                ? conventions.ToArray()
                : null;
        }

        void ConfigureActivator()
        {
            if (activator == null)
                return;

            var instance = (IActorActivator) System.Activator.CreateInstance(activator.Item1);
            instance.Init(activator.Item2);

            ActorType.Activator = instance;
        }

        void ConfigureInterceptor()
        {
            if (interceptor == null)
                return;

            var instance = (IInterceptor) System.Activator.CreateInstance(interceptor.Item1);
            instance.Install(InvocationPipeline.Instance, interceptor.Item2);
        }

        void RegisterInterfaces() => ActorInterface.Register(assemblies, interfaces);

        void RegisterTypes() => ActorType.Register(assemblies);

        void RegisterAutoruns()
        {
            var autoruns = new Dictionary<string, string[]>();

            foreach (var actor in assemblies.SelectMany(x => x.ActorTypes()))
            {
                var ids = AutorunAttribute.From(actor);
                if (ids.Length > 0)
                    autoruns.Add(ActorTypeName.Of(actor), ids);
            }

            Bootstrapper<AutorunBootstrapper>(autoruns);
        }

        void RegisterStorageProviders()
        {
            Configuration.Globals.RegisterStorageProvider<GrainFactoryProvider>("#ORLKKA_GFP");
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
            foreach (var actor in assemblies.SelectMany(x => x.ActorTypes()))
                ActorBehavior.Register(actor);
        }

        void RegisterStreamSubscriptions()
        {
            foreach (var actor in ActorType.Registered())
                StreamSubscriptionMatcher.Register(actor.Subscriptions());

            const string id = "stream-subscription-boot";

            var properties = new Dictionary<string, string>();
            properties["providers"] = string.Join(";", streamProviders
                .Where(x => x.IsPersistentStreamProvider())
                .Select(x => x.Name));

            Configuration.Globals.RegisterStorageProvider<StreamSubscriptionBootstrapper>(id, properties);
        }

        public override object InitializeLifetimeService()
        {
            return null;
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