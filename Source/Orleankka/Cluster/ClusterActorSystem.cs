using System;
using System.Linq;
using System.Net;

using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    public class ClusterActorSystem : MarshalByRefObject, IActorSystem
    {
        public static IActorSystem Current 
        {
            get
            {
                var instance = AppDomain.CurrentDomain.GetData("ActorSystem.Current") as IActorSystem;
                
                if (instance == null)
                    throw new InvalidOperationException("Cluster actor system hasn't been initialized");

                return instance;
            }
            internal set
            {
                AppDomain.CurrentDomain.SetData("ActorSystem.Current", value);
            }
        }

        readonly IDisposable configurator;
        SiloHost host;

        internal ClusterActorSystem(IDisposable configurator, ClusterConfiguration configuration)
        {
            this.configurator = configurator;
            host = new SiloHost(Dns.GetHostName(), configuration);
            Current = this;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return ActorRef.Resolve(path);
        }

        internal void Start()
        {
            host.LoadOrleansConfig();
            host.InitializeOrleansSilo();
            host.StartOrleansSilo();
        }

        void IDisposable.Dispose()
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