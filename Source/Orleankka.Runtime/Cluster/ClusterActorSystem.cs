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

        internal static bool Initialized => current != null;

        readonly IDisposable configurator;

        internal ClusterActorSystem(IDisposable configurator, ClusterConfiguration configuration)
        {
            current = this;
            this.configurator = configurator;
            Host = new SiloHost(Dns.GetHostName(), configuration);
        }

        public bool Started { get; private set; }
        public SiloHost Host { get; private set; }
        public Silo Silo { get; private set; }

        public void Start(bool wait = false)
        {
            if (Started)
                throw new InvalidOperationException("Cluster already started");

            Host.LoadOrleansConfig();
            Host.InitializeOrleansSilo();

            var siloField = typeof(SiloHost).GetField("orleans", BindingFlags.Instance | BindingFlags.NonPublic);
            if (siloField == null)
                throw new InvalidOperationException("Hey, who moved my cheese? SiloHost don't have private 'orleans' field anymore!");

            Silo = (Silo)siloField.GetValue(Host);

            if (!Host.StartOrleansSilo(catchExceptions: false))
                throw new Exception("Silo failed to start. Check the logs");

            Started = true;

            if (wait)
                Host.WaitForOrleansSiloShutdown();
        }

        public void Stop(bool force = false)
        {
            if (!Started)
                throw new InvalidOperationException("Cluster already stopped");

            if (force)
                Host.StopOrleansSilo();
            else
                Host.ShutdownOrleansSilo();

            Host.UnInitializeOrleansSilo();

            Started = false;
        }

        public override void Dispose()
        {
            if (Started)
                Stop(true);

            if (Host == null)
                return;

            Host.Dispose();
            Host = null;

            configurator.Dispose();
            current = null;
        }
    }
}