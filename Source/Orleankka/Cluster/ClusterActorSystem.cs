using System;
using System.Net;

using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    public class ClusterActorSystem : ActorSystem
    {
        static IActorSystem current;

        public static IActorSystem Current 
        {
            get
            {
                if (!Initialized)
                    throw new InvalidOperationException("Cluster actor system hasn't been initialized");

                return current;
            }

            internal set
            {
                current = value;
            }
        }

        public static bool Initialized => current != null;

        readonly IDisposable configurator;
        SiloHost host;

        internal ClusterActorSystem(IDisposable configurator, ClusterConfiguration configuration)
        {
            current = this;
            this.configurator = configurator;
            host = new SiloHost(Dns.GetHostName(), configuration);
        }

        internal void Start()
        {
            host.LoadOrleansConfig();
            host.InitializeOrleansSilo();

            if (!host.StartOrleansSilo()) // weird decision made by Orleans team (what about fail fast?)
                throw new Exception("Silo failed to start. Check the logs");
        }

        public override void Dispose()
        {
            if (host == null)
                return;

            host.StopOrleansSilo();
            host.UnInitializeOrleansSilo();
            host.Dispose();
            host = null;

            configurator.Dispose();
            current = null;
        }
    }
}