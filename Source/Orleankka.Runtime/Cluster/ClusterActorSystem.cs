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
    using Utility;

     public class ClusterActorSystem : ActorSystem
    {
        internal readonly ActorInvocationPipeline Pipeline;

        internal ClusterActorSystem(
            ClusterConfiguration configuration,
            Action<ISiloHostBuilder> builder,
            string[] persistentStreamProviders,
            Assembly[] assemblies,
            Action<IServiceCollection> di,
            ActorInvocationPipeline pipeline,
            IActorRefInvoker invoker)
            : base(invoker)
        {
            Pipeline = pipeline;

            using (Trace.Execution("Orleans silo initialization"))
            {
                var sb = new SiloHostBuilder();
                sb.UseConfiguration(configuration);

                Register(sb, new[]
                {
                    typeof(ActorRef).Assembly,
                    typeof(Actor).Assembly,
                    ActorInterfaceDeclaration.GeneratedAssembly(),
                    ActorTypeDeclaration.GeneratedAssembly()
                });

                builder?.Invoke(sb);

                sb.ConfigureServices(services =>
                {
                    BootStreamSubscriptions(services, persistentStreamProviders);

                    services.AddSingleton<IActorSystem>(this);
                    services.AddSingleton(this);
                    services.TryAddSingleton<IActorActivator>(x => new DefaultActorActivator(x));
                    services.TryAddSingleton<Func<IIncomingGrainCallContext, string>>(DashboardIntegration.Format);

                    di?.Invoke(services);
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
    }
}