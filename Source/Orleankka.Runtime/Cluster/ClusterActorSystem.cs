using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Orleans.Runtime;
using Orleans.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Storage;

namespace Orleankka.Cluster
{
    using Core;
    using Core.Streams;
    using Facets;
    using Utility;

     public class ClusterActorSystem : ActorSystem, IDisposable
    {
        internal readonly ActorMiddlewarePipeline Pipeline;

        internal ClusterActorSystem(
            ClusterConfiguration configuration,
            Action<ISiloHostBuilder> builder,
            string[] persistentStreamProviders,
            Assembly[] assemblies,
            Assembly[] generated,
            Action<IServiceCollection> di,
            ActorMiddlewarePipeline pipeline,
            IActorRefMiddleware middleware)
            : base(middleware)
        {
            Pipeline = pipeline;

            using (Trace.Execution("Orleans silo initialization"))
            {
                var sb = new SiloHostBuilder();
                sb.UseConfiguration(configuration);

                builder?.Invoke(sb);

                Register(sb, new[] {typeof(ActorRef).Assembly, typeof(Actor).Assembly});
                Register(sb, generated.Distinct());

                sb.ConfigureServices(services =>
                {
                    di?.Invoke(services);

                    BootStreamSubscriptions(services, persistentStreamProviders);

                    services.AddSingleton<IActorSystem>(this);
                    services.AddSingleton(this);

                    services.TryAdd(new ServiceDescriptor(typeof(IGrainActivator), sp => new DefaultGrainActivator(sp), ServiceLifetime.Singleton));
                    services.Decorate<IGrainActivator>(inner => new ActorGrainActivator(inner));

                    services.TryAddSingleton<Func<IIncomingGrainCallContext, string>>(DashboardIntegration.Format);

                    // storage feature facet attribute mapper
                    services.AddSingleton(new StorageProviderFacet());
                    services.AddSingleton(typeof(IAttributeToFactoryMapper<UseStorageProviderAttribute>), sp => new UseStorageProviderAttributeMapper(sp));
                });

                Register(sb, assemblies.Distinct());

                Host = sb.Build();
            }

            Silo = Host.Services.GetRequiredService<Silo>();
            Initialize(Host.Services);
        }

         static void Register(ISiloHostBuilder sb, IEnumerable<Assembly> parts) => sb.ConfigureApplicationParts(apm =>
         {
             foreach (var part in parts)
                 apm.AddApplicationPart(part)
                    .WithCodeGeneration();
         });

         static void BootStreamSubscriptions(IServiceCollection services, string[] persistentStreamProviders)
         {
             const string name = "orlssb";
             services.AddOptions<StreamSubscriptionBootstrapperOptions>(name).Configure(c => c.Providers = persistentStreamProviders);
             services.AddSingletonNamedService(name, StreamSubscriptionBootstrapper.Create);
             services.AddSingletonNamedService(name, (s, n) => (ILifecycleParticipant<ISiloLifecycle>) s.GetRequiredServiceByName<IGrainStorage>(n));
         }

        public ISiloHost Host { get; }
        public Silo Silo { get; }

        public async Task Start()
        {
            using (Trace.Execution("Orleans silo startup"))
                await Host.StartAsync();
        }

        public async Task Stop()
        {
            using (Trace.Execution("Orleans silo shutdown"))
                await Host.StopAsync();
        }

         /// <inheritdoc />
         public void Dispose() => Host?.Dispose();
    }
}