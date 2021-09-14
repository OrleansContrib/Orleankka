using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Orleans;
using Orleans.ApplicationParts;
using Orleans.Hosting;

namespace Orleankka.Client
{
    public static class ClientBuilderExtension
    {
        public static IClientBuilder UseOrleankka(this IClientBuilder builder) => 
            builder
                .ConfigureServices(services => Configure(builder, services))
                .ConfigureApplicationParts(apm => apm
                    .AddApplicationPart(typeof(IClientEndpoint).Assembly)
                    .WithCodeGeneration());

        static void Configure(IClientBuilder builder, IServiceCollection services)
        {
            var assemblies = builder.GetApplicationPartManager().ApplicationParts
                .OfType<AssemblyPart>().Select(x => x.Assembly)
                .ToArray();

            services.TryAddSingleton<IActorRefMiddleware>(DefaultActorRefMiddleware.Instance);
            services.TryAddSingleton<IStreamRefMiddleware>(DefaultStreamRefMiddleware.Instance);

            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClientActorSystem>());
            services.AddSingleton<IClientActorSystem>(sp => sp.GetService<ClientActorSystem>());

            services.AddSingleton(sp => new ClientActorSystem(assemblies, sp));
        }

        public static IClientActorSystem ActorSystem(this IClusterClient client) => client.ServiceProvider.GetRequiredService<IClientActorSystem>();
    }
}