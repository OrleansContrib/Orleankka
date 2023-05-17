using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Orleans;
using Orleans.Hosting;

namespace Orleankka.Client
{
    using Orleans.Serialization;

    using System;

    public static class ClientBuilderExtension
    {
        public static IClientBuilder UseOrleankka(this IClientBuilder builder) => 
            builder.ConfigureServices(services => Configure(builder, services));

        static void Configure(IClientBuilder builder, IServiceCollection services)
        {
            var assemblies = services.GetRelevantAssemblies();

            services.TryAddSingleton<IActorRefMiddleware>(DefaultActorRefMiddleware.Instance);

            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClientActorSystem>());
            services.AddSingleton<IClientActorSystem>(sp => sp.GetService<ClientActorSystem>());

            services.AddSingleton(sp => new ClientActorSystem(assemblies, sp));
        }

        public static IClientActorSystem ActorSystem(this IClusterClient client) => client.ServiceProvider.GetRequiredService<IClientActorSystem>();
    }
}