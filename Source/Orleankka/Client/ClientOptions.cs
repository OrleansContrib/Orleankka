using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.ApplicationParts;
using Orleans.Hosting;

namespace Orleankka.Client
{
    using Utility;

    public sealed class OrleankkaClientOptions
    {
        IActorRefMiddleware middleware;

        /// <summary>
        /// Registers global <see cref="ActorRef"/> middleware (interceptor)
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        public OrleankkaClientOptions ActorRefMiddleware(IActorRefMiddleware middleware)
        {
            Requires.NotNull(middleware, nameof(middleware));

            if (this.middleware != null)
                throw new InvalidOperationException("ActorRef middleware has been already registered");

            this.middleware = middleware;
            return this;
        }

        internal void Configure(IClientBuilder builder, IServiceCollection services)
        {
            var assemblies = builder.GetApplicationPartManager().ApplicationParts
                                    .OfType<AssemblyPart>().Select(x => x.Assembly)
                                    .ToArray();

            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClientActorSystem>());
            services.AddSingleton<IClientActorSystem>(sp => sp.GetService<ClientActorSystem>());

            services.AddSingleton(sp => new ClientActorSystem(assemblies, sp, middleware));
        }
    }

    public static class ClientBuilderExtension
    {
        public static IClientBuilder UseOrleankka(this IClientBuilder builder) => 
            UseOrleankka(builder, new OrleankkaClientOptions());

        public static IClientBuilder UseOrleankka(this IClientBuilder builder, Func<OrleankkaClientOptions, OrleankkaClientOptions> configure) => 
            UseOrleankka(builder, configure(new OrleankkaClientOptions()));

        public static IClientBuilder UseOrleankka(this IClientBuilder builder, OrleankkaClientOptions cfg) => 
            builder
                .ConfigureServices(services => cfg.Configure(builder, services))
                .ConfigureApplicationParts(apm => apm
                    .AddApplicationPart(typeof(IClientEndpoint).Assembly)
                    .WithCodeGeneration());

        public static IClientActorSystem ActorSystem(this IClusterClient client) => client.ServiceProvider.GetRequiredService<IClientActorSystem>();
    }
}