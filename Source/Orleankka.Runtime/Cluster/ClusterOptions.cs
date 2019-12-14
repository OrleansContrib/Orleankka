using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.CodeGeneration;
using Orleans.Configuration.Overrides;
using Orleans.Hosting;
using Orleans.Runtime;

namespace Orleankka.Cluster
{
    using Client;
    using Utility;

    public sealed class OrleankkaClusterOptions
    {
        IActorRefMiddleware actorRefMiddleware;
        IActorMiddleware actorMiddleware;

        /// <summary>
        /// Registers global actor middleware (interceptor).
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        public OrleankkaClusterOptions ActorMiddleware(IActorMiddleware middleware)
        {
            actorMiddleware = middleware;
            return this;
        }

        /// <summary>
        /// Registers global cluster-wide <see cref="ActorRef"/> middleware (interceptor)
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        public OrleankkaClusterOptions ActorRefMiddleware(IActorRefMiddleware middleware)
        {
            Requires.NotNull(middleware, nameof(middleware));

            if (actorRefMiddleware != null)
                throw new InvalidOperationException("ActorRef middleware for cluster has been already registered");

            actorRefMiddleware = middleware;
            return this;
        }
        
        internal void Configure(IApplicationPartManager apm, IServiceCollection services)
        {
            var assemblies = apm.ApplicationParts
                .OfType<AssemblyPart>().Select(x => x.Assembly)
                .ToArray();

            services.AddSingleton(sp => new ClusterActorSystem(assemblies, sp, actorRefMiddleware, actorMiddleware));
            services.AddSingleton(sp => new ClientActorSystem(assemblies, sp, actorRefMiddleware));

            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClusterActorSystem>());
            services.AddSingleton<IClientActorSystem>(sp => sp.GetService<ClientActorSystem>());

            services.TryAddSingleton<IDispatcherRegistry>(x => BuildDispatcherRegistry(x, assemblies));
            services.TryAddSingleton<Func<MethodInfo, InvokeMethodRequest, IGrain, string>>(DashboardIntegration.Format);

            services.TryAdd(new ServiceDescriptor(typeof(IGrainActivator), sp => new DefaultGrainActivator(sp), ServiceLifetime.Singleton));
            services.Decorate<IGrainActivator>(inner => new ActorGrainActivator(inner));

            services.AddTransient<IConfigurationValidator, DispatcherOptionsValidator>();
            services.AddOptions<DispatcherOptions>();
        }

        DispatcherRegistry BuildDispatcherRegistry(IServiceProvider services, IEnumerable<Assembly> assemblies)
        {
            var dispatchActors = assemblies.SelectMany(x => x.GetTypes())
                .Where(x => typeof(DispatchActorGrain).IsAssignableFrom(x) && !x.IsAbstract);

            var dispatcherRegistry = new DispatcherRegistry();
            var options = services.GetService<IOptions<DispatcherOptions>>().Value;

            foreach (var actor in dispatchActors)
                dispatcherRegistry.Register(actor, new Dispatcher(actor, options.HandlerNamingConventions, options.RootTypes));

            return dispatcherRegistry;
        }
    }

    public static class SiloHostBuilderExtension
    {
        public static ISiloHostBuilder UseOrleankka(this ISiloHostBuilder builder) => 
            UseOrleankka(builder, new OrleankkaClusterOptions());

        public static ISiloHostBuilder UseOrleankka(this ISiloHostBuilder builder, Func<OrleankkaClusterOptions, OrleankkaClusterOptions> configure) => 
            UseOrleankka(builder, configure(new OrleankkaClusterOptions()));

        public static ISiloHostBuilder UseOrleankka(this ISiloHostBuilder builder, OrleankkaClusterOptions cfg) => 
            builder.ConfigureServices(services => UseOrleankka(builder.GetApplicationPartManager(), services, cfg));

        public static ISiloBuilder UseOrleankka(this ISiloBuilder builder) => 
            UseOrleankka(builder, new OrleankkaClusterOptions());

        public static ISiloBuilder UseOrleankka(this ISiloBuilder builder, Func<OrleankkaClusterOptions, OrleankkaClusterOptions> configure) => 
            UseOrleankka(builder, configure(new OrleankkaClusterOptions()));

        public static ISiloBuilder UseOrleankka(this ISiloBuilder builder, OrleankkaClusterOptions cfg) => 
            builder.ConfigureServices(services => UseOrleankka(builder.GetApplicationPartManager(), services, cfg));

        static void UseOrleankka(IApplicationPartManager apm, IServiceCollection services, OrleankkaClusterOptions cfg)
        {
            cfg.Configure(apm, services);
            apm.AddApplicationPart(typeof(IClientEndpoint).Assembly).WithCodeGeneration();
        }

        public static IClientActorSystem ActorSystem(this ISiloHost host) => host.Services.GetRequiredService<IClientActorSystem>();
    }
}