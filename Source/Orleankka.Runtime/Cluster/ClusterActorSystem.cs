using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Orleans;
using Orleans.CodeGeneration;

namespace Orleankka.Cluster
{
    using Core;
    using Utility;

     public class ClusterActorSystem : ActorSystem
    {
        internal readonly ActorInvocationPipeline Pipeline;

        internal ClusterActorSystem(
            ClusterConfiguration configuration,
            Assembly[] assemblies,
            Action<IServiceCollection> di,
            ActorInvocationPipeline pipeline,
            IActorRefInvoker invoker)
            : base(invoker)
        {
            Pipeline = pipeline;

            using (Trace.Execution("Orleans silo initialization"))
            {
                var builder = new SiloHostBuilder()
                    .UseConfiguration(configuration)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton<IActorSystem>(this);
                        services.AddSingleton(this);
                        services.TryAddSingleton<IActorActivator>(x => new DefaultActorActivator(x));
                        services.AddSingleton<Func<MethodInfo, InvokeMethodRequest, IGrain, string>>(DashboardIntegration.Format);

                        di?.Invoke(services);
                    });

                var parts = new List<Assembly>(assemblies);
                parts.AddRange(ActorInterface.Registered().Select(x => x.Grain.Assembly).Distinct());
                parts.AddRange(ActorType.Registered().Select(x => x.Grain.Assembly).Distinct());

                builder.ConfigureApplicationParts(m =>
                {
                    var asm = m.AddFromAppDomain().WithReferences();
                    parts.ForEach(x => asm.AddApplicationPart(x));
                    asm.WithCodeGeneration();
                });

                Host = builder.Build();
            }

            Silo = Host.Services.GetRequiredService<Silo>();
            Initialize(Host.Services);
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