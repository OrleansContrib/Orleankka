using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.ApplicationParts;
using Orleans.CodeGeneration;
using Orleans.Hosting;

namespace Orleankka.Cluster
{
    using Utility;

    public sealed class OrleankkaClusterOptions
    {
        readonly HashSet<string> conventions = 
             new HashSet<string>();

        readonly ActorMiddlewarePipeline pipeline = 
             new ActorMiddlewarePipeline();
        
        IActorRefMiddleware middleware;

        /// <summary>
        /// Registers global actor middleware (interceptor). This middleware will be used for every actor 
        /// which doesn't specify an individual middleware via call to <see cref="ActorMiddleware(Type,IActorMiddleware) "/>.
        /// </summary>
        /// <param name="global">The middleware.</param>
        public OrleankkaClusterOptions ActorMiddleware(IActorMiddleware global)
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
        public OrleankkaClusterOptions ActorMiddleware(Type type, IActorMiddleware middleware)
        {
            pipeline.Register(type, middleware);
            return this;
        }

        /// <summary>
        /// Registers global <see cref="ActorRef"/> middleware (interceptor)
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        public OrleankkaClusterOptions ActorRefMiddleware(IActorRefMiddleware middleware)
        {
            Requires.NotNull(middleware, nameof(middleware));

            if (this.middleware != null)
                throw new InvalidOperationException("ActorRef middleware has been already registered");

            this.middleware = middleware;
            return this;
        }

        public OrleankkaClusterOptions HandlerNamingConventions(params string[] conventions)
        {
            Requires.NotNull(conventions, nameof(conventions));

            if (conventions.Length == 0)
                throw new ArgumentException("conventions are empty", nameof(conventions));

            Array.ForEach(conventions, x => this.conventions.Add(x));

            return this;
        }

        internal void Configure(ISiloHostBuilder builder, IServiceCollection services)
        {
            var assemblies = builder.GetApplicationPartManager().ApplicationParts
                                    .OfType<AssemblyPart>().Select(x => x.Assembly)
                                    .ToArray();

            services.AddSingleton(sp => new ClusterActorSystem(assemblies, sp, pipeline, middleware));
            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClusterActorSystem>());
            services.AddSingleton<Func<MethodInfo, InvokeMethodRequest, IGrain, string>>(DashboardIntegration.Format);
            services.AddSingleton<IDispatcherRegistry>(BuildDispatcherRegistry(assemblies));
        }

        DispatcherRegistry BuildDispatcherRegistry(IEnumerable<Assembly> assemblies)
        {
            var dispatchActors = assemblies.SelectMany(x => x.GetTypes())
                                           .Where(x => typeof(DispatchActorGrain).IsAssignableFrom(x) && !x.IsAbstract);

            var dispatcherRegistry = new DispatcherRegistry();
            var handlerNamingConventions = conventions.Count > 0 ? conventions.ToArray() : null;

            foreach (var actor in dispatchActors)
                dispatcherRegistry.Register(actor, new Dispatcher(actor, handlerNamingConventions));

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
            builder
            .ConfigureServices(services => cfg.Configure(builder, services))
            .ConfigureApplicationParts(apm => apm
                .AddApplicationPart(typeof(IClientEndpoint).Assembly)
                .WithCodeGeneration());
    }
}