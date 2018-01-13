using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.CodeGeneration;
using Orleans.Hosting;
using Orleans.Internals;
using Orleans.Runtime.Configuration;
using Orleans.Streams;

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

        readonly HashSet<string> conventions = 
             new HashSet<string>();

        readonly HashSet<BootstrapProviderConfiguration> bootstrapProviders =
             new HashSet<BootstrapProviderConfiguration>();

        readonly ActorInvocationPipeline pipeline = 
             new ActorInvocationPipeline();
        
        IActorRefInvoker invoker;

        public ClusterConfigurator Bootstrapper<T>(object properties = null) where T : IBootstrapper
        {
            var configuration = new BootstrapProviderConfiguration(typeof(T), properties);

            if (!bootstrapProviders.Add(configuration))
                throw new ArgumentException($"Bootstrapper of the type {typeof(T)} has been already registered");

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

        internal void Configure(ISiloHostBuilder builder, IServiceCollection services)
        {
            var cluster = GetClusterConfiguration(services);

            RegisterAssemblies(builder);
            RegisterInterfaces();
            RegisterTypes();
            RegisterAutoruns();
            
            var persistentStreamProviders = RegisterStreamProviders(cluster);
            RegisterStreamSubscriptions(cluster, persistentStreamProviders);

            RegisterBootstrappers(cluster);
            RegisterBehaviors();
            RegisterDependencies(services);
        }

        void RegisterAssemblies(ISiloHostBuilder builder) => 
            registry.Register(builder.GetApplicationPartManager(), x => x.ActorTypes());

        static ClusterConfiguration GetClusterConfiguration(IServiceCollection services)
        {
            if (!(services.SingleOrDefault(service =>
                service.ServiceType == typeof(ClusterConfiguration)).ImplementationInstance is ClusterConfiguration configuration))
                throw new InvalidOperationException("Cannot configure Orleankka before cluster configuration is set");

            return configuration;
        }

        void RegisterDependencies(IServiceCollection services)
        {
            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClusterActorSystem>());

            services.AddSingleton(sp => new ClusterActorSystem(
                sp.GetService<IStreamProviderManager>(), 
                sp.GetService<IGrainFactory>(), 
                pipeline, invoker));

            services.AddSingleton<Func<MethodInfo, InvokeMethodRequest, IGrain, string>>(DashboardIntegration.Format);
        }

        void RegisterInterfaces() => ActorInterface.Register(registry.Mappings);

        void RegisterTypes() => ActorType.Register(registry.Assemblies, conventions.Count > 0 ? conventions.ToArray() : null);

        void RegisterAutoruns()
        {
            var autoruns = new Dictionary<string, string[]>();

            foreach (var actor in registry.Assemblies.SelectMany(x => x.ActorTypes()))
            {
                var ids = AutorunAttribute.From(actor);
                if (ids.Length <= 0) 
                    continue;
                
                var typeName = ActorTypeName.Of(actor);
                if (registry.IsRegistered(typeName))
                    autoruns.Add(typeName, ids);
            }

            Bootstrapper<AutorunBootstrapper>(autoruns);
        }

        static string[] RegisterStreamProviders(ClusterConfiguration configuration)
        {
            Type GetType(string partialTypeName)
            {
                var type = Type.GetType(partialTypeName);
                if (type != null) 
                    return type;

                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(partialTypeName);
                    if (type != null)
                        return type;
                }
                
                return null ;
            }

            if (!configuration.Globals.ProviderConfigurations.TryGetValue(ProviderCategoryConfiguration.STREAM_PROVIDER_CATEGORY_NAME, out ProviderCategoryConfiguration pcc))
                return Array.Empty<string>();

            var persistentStreamProviders = new List<string>();
            var allStreamProviders = pcc.Providers.ToArray();
            
            foreach (var each in allStreamProviders)
            {                
                var provider = each.Value;
             
                var type = GetType(provider.Type);
                if (type.IsPersistentStreamProvider())
                {
                    persistentStreamProviders.Add(each.Key);
                    continue;
                }

                pcc.Providers.Remove(each.Key);

                var properties = provider.Properties.ToDictionary(k => k.Key, v => v.Value);
                var spc = new StreamProviderConfiguration(provider.Name, type, properties);
                spc.Register(configuration);
            }

            return persistentStreamProviders.ToArray();
        }

        void RegisterBootstrappers(ClusterConfiguration cluster)
        {
            foreach (var each in bootstrapProviders)
                each.Register(cluster.Globals);
        }

        void RegisterBehaviors()
        {
            foreach (var actor in registry.Assemblies.SelectMany(x => x.ActorTypes()))
                ActorBehavior.Register(actor);
        }

        static void RegisterStreamSubscriptions(ClusterConfiguration cluster, IEnumerable<string> persistentStreamProviders)
        {
            foreach (var actor in ActorType.Registered())
                StreamSubscriptionMatcher.Register(actor.Name, actor.Subscriptions());

            const string id = "stream-subscription-boot";

            var properties = new Dictionary<string, string>
            {
                ["providers"] = string.Join(";", persistentStreamProviders)
            };

            cluster.Globals.RegisterStorageProvider<StreamSubscriptionBootstrapper>(id, properties);
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

    public static class SiloHostBuilderExtension
    {
        public static ISiloHostBuilder ConfigureOrleankka(this ISiloHostBuilder builder) => 
            ConfigureOrleankka(builder, new ClusterConfigurator());

        public static ISiloHostBuilder ConfigureOrleankka(this ISiloHostBuilder builder, Func<ClusterConfigurator, ClusterConfigurator> configure) => 
            ConfigureOrleankka(builder, configure(new ClusterConfigurator()));

        public static ISiloHostBuilder ConfigureOrleankka(this ISiloHostBuilder builder, ClusterConfigurator cfg) => 
            builder
            .ConfigureServices(services => cfg.Configure(builder, services))
            .ConfigureApplicationParts(apm => apm
                .AddApplicationPart(typeof(IClientEndpoint).Assembly)
                .WithCodeGeneration());

        public static IActorSystem ActorSystem(this ISiloHost host) => host.Services.GetRequiredService<IActorSystem>();
    }
}