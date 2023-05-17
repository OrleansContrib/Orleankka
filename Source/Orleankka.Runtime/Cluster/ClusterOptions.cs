using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime;

namespace Orleankka.Cluster
{
    using Client;

    public static class SiloHostBuilderExtension
    {
        public static ISiloBuilder UseOrleankka(this ISiloBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                services.AddSingleton(sp => new ClusterActorSystem(assemblies, sp));
                services.AddSingleton(sp => new ClientActorSystem(assemblies, sp));

                services.AddSingleton<IActorSystem>(sp => sp.GetService<ClusterActorSystem>());
                services.AddSingleton<IClientActorSystem>(sp => sp.GetService<ClientActorSystem>());

                services.TryAddSingleton<IDispatcherRegistry>(x => BuildDispatcherRegistry(x, assemblies));
                services.TryAddSingleton<Func<MethodInfo, RequestBase, IGrain, string>>(DashboardIntegration.Format);

                services.TryAddSingleton<IActorMiddleware>(DefaultActorMiddleware.Instance);
                services.TryAddSingleton<IActorRefMiddleware>(DefaultActorRefMiddleware.Instance);

                services.Decorate<IGrainActivator>(inner => new ActorGrainActivator(inner));

                services.AddTransient<IConfigurationValidator, DispatcherOptionsValidator>();
                services.AddOptions<DispatcherOptions>();
            });
        }

        static DispatcherRegistry BuildDispatcherRegistry(IServiceProvider services, IEnumerable<Assembly> assemblies)
        {
            var dispatchActors = assemblies.SelectMany(x => x.GetTypes())
                .Where(x => typeof(DispatchActorGrain).IsAssignableFrom(x) && !x.IsAbstract);

            var dispatcherRegistry = new DispatcherRegistry();
            var options = services.GetService<IOptions<DispatcherOptions>>().Value;

            foreach (var actor in dispatchActors)
                dispatcherRegistry.Register(actor, new Dispatcher(actor, options.HandlerNamingConventions, options.RootTypes));

            return dispatcherRegistry;
        }

        public static IClientActorSystem ActorSystem(this IHost host) => host.Services.GetRequiredService<IClientActorSystem>();
    }
}