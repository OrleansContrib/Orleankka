using System;
using System.Linq;
using System.Net;

using Orleans.Internals;
using Orleans.Runtime;
using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orleankka.Cluster
{
    using Core;
    using Utility;

     public class ClusterActorSystem : ActorSystem, IDisposable
    {
        readonly Action<IServiceCollection> di;
        internal readonly ActorInvocationPipeline Pipeline;

        [ThreadStatic]
        static ClusterActorSystem current;

        internal ClusterActorSystem(ClusterConfiguration configuration, Action<IServiceCollection> di, ActorInvocationPipeline pipeline, IActorRefInvoker invoker)
            : base(invoker)
        {
            this.di = di;
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
                current.di?.Invoke(services);

                services.AddSingleton<IActorSystem>(current);
                services.AddSingleton(current);
                services.TryAddSingleton<IActorActivator>(x => new DefaultActorActivator(x));

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