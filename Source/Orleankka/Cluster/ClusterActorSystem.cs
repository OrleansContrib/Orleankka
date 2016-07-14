using System;
using System.Net;
using System.Reflection;

using Orleans.Runtime;
using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    public class ClusterActorSystem : ActorSystem
    {
        static ClusterActorSystem current;

        internal static ClusterActorSystem Current 
        {
            get
            {
                if (!Initialized)
                    throw new InvalidOperationException("Cluster actor system hasn't been initialized");

                return current;
            }
        }

        public static bool Initialized => current != null;

        readonly IDisposable configurator;

        internal ClusterActorSystem(IDisposable configurator, ClusterConfiguration configuration)
        {
            current = this;
            this.configurator = configurator;
            Host = new SiloHost(Dns.GetHostName(), configuration);
        }

        public SiloHost Host { get; private set; }
        public Silo Silo { get; private set; }

        internal void Start(bool wait)
        {
            Host.LoadOrleansConfig();
            Host.InitializeOrleansSilo();

            var siloField = typeof(SiloHost).GetField("orleans", BindingFlags.Instance | BindingFlags.NonPublic);
            if (siloField == null)
                throw new InvalidOperationException("Hey, who moved my cheese? SiloHost don't have private 'orleans' field anymore!");

            Silo = (Silo)siloField.GetValue(Host);

            if (!Host.StartOrleansSilo(catchExceptions: false))
                throw new Exception("Silo failed to start. Check the logs");

            if (wait)
                Host.WaitForOrleansSiloShutdown();
        }

        public override void Dispose()
        {
            if (Host == null)
                return;

            Host.StopOrleansSilo();
            Host.UnInitializeOrleansSilo();
            Host.Dispose();
            Host = null;

            configurator.Dispose();
            current = null;
        }
    }
}