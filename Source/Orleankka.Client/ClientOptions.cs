using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Orleans.Serialization;

namespace Orleankka.Client
{
    public static class ClientBuilderExtension
    {
        public static IHostBuilder UseOrleankka(this IHostBuilder builder) => 
            builder.ConfigureServices((_, services) => Configure(builder, services));

        static void Configure(IHostBuilder builder, IServiceCollection services)
        {
            var assemblies = services.GetRelevantAssemblies();

            services.TryAddSingleton<IActorRefMiddleware>(DefaultActorRefMiddleware.Instance);
            services.TryAddSingleton<IStreamRefMiddleware>(DefaultStreamRefMiddleware.Instance);

            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClientActorSystem>());
            services.AddSingleton<IClientActorSystem>(sp => sp.GetService<ClientActorSystem>());

            services.AddSingleton(sp => new ClientActorSystem(assemblies, sp));
        }

        public static IClientActorSystem ActorSystem(this IHost client) => client.Services.GetRequiredService<IClientActorSystem>();
    }
}