using System;
using System.Net;

using Microsoft.Extensions.DependencyInjection;

using Orleans.Internals;
using Orleans.Runtime;
using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    using Core;
    using Utility;

    public class ClusterActorSystem : ActorSystem, IDisposable
    {
        internal readonly ActorInvocationPipeline Pipeline;
        internal readonly IActorActivator Activator;

        [ThreadStatic]
        static ClusterActorSystem current;

        internal ClusterActorSystem(ClusterConfiguration configuration, ActorInvocationPipeline pipeline, IActorActivator activator, IActorRefInvoker invoker)
            : base(invoker)
        {
            Activator = activator ?? new DefaultActorActivator();
            Pipeline = pipeline;

            current = this;
            configuration.UseStartupType<Startup>();

            using (Trace.Execution("Orleans silo initialization"))
            {
                Host = new SiloHost(Dns.GetHostName(), configuration);
                Host.LoadOrleansConfig();
                Host.InitializeOrleansSilo();
            }

            Silo = Host.GetSilo();
            Initialize(Silo.GetServiceProvider());
        }

        class Startup
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<IActorSystem>(current);
                services.AddSingleton(current);

                return services.BuildServiceProvider();
            }
        }

        public bool Started => Host.IsStarted;

        public SiloHost Host { get; }
        public Silo Silo { get; }

        public void Start(bool wait = false)
        {
            if (Started)
                throw new InvalidOperationException("Cluster already started");

            using (Trace.Execution("Orleans silo startup"))
                if (!Host.StartOrleansSilo(catchExceptions: false))
                    throw new Exception("Silo failed to start. Check the logs");
            
            if (wait)
                Host.WaitForOrleansSiloShutdown();
        }

        public void Stop(bool force = false)
        {
            if (!Started)
                throw new InvalidOperationException("Cluster already stopped");

            if (force)
            {
                Host.StopOrleansSilo();
                return;
            }

            Host.ShutdownOrleansSilo();
        }

        public void Dispose()
        {
            Host?.UnInitializeOrleansSilo();
            Host?.Dispose();
        }
    }
}