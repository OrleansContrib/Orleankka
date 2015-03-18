using System;
using System.Linq;
using System.Net;

using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    class ClusterActorSystem : MarshalByRefObject, IActorSystem
    {
        readonly IDisposable configurator;
        SiloHost host;

        internal ClusterActorSystem(AppDomain domain, IDisposable configurator, ClusterConfiguration configuration)
        {
            this.configurator = configurator;
            host = new SiloHost(Dns.GetHostName(), configuration);
            domain.SetData("ActorSystem.Current", this);
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return ActorRef.Resolve(path);
        }

        public void Start()
        {
            host.LoadOrleansConfig();
            host.InitializeOrleansSilo();
            host.StartOrleansSilo();
        }

        public void Dispose()
        {
            if (host == null)
                return;

            host.StopOrleansSilo();
            host.UnInitializeOrleansSilo();
            host.Dispose();
            host = null;

            configurator.Dispose();
        }
    }
}