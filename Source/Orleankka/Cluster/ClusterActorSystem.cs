using System;
using System.Linq;

using Orleans.Runtime.Host;

namespace Orleankka.Cluster
{
    public sealed class ClusterActorSystem : MarshalByRefObject, IActorSystem
    {
        readonly IActorSystemConfigurator configurator;
        SiloHost host;

        public ClusterActorSystem(IActorSystemConfigurator configurator, SiloHost host)
        {
            this.configurator = configurator;
            this.host = host;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return ActorRef.Resolve(path);
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