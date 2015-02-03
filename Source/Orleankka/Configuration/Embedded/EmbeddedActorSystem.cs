using System;
using System.Linq;

using Orleans.Runtime.Host;

namespace Orleankka.Configuration.Embedded
{
    class EmbeddedActorSystem : IActorSystem
    {
        readonly IActorSystem system;
        readonly AppDomain domain;
        SiloHost host;

        internal EmbeddedActorSystem(IActorSystem system, AppDomain domain, SiloHost host)
        {
            this.system = system;
            this.domain = domain;
            this.host = host;
        }

        public void Dispose()
        {
            if (host == null)
                return;

            host.StopOrleansSilo();                
            host.Dispose();
            host = null;

            AppDomain.Unload(domain);
        }

        public ActorRef ActorOf(ActorPath path)
        {
            return system.ActorOf(path);
        }

        public ObserverRef ObserverOf(ObserverPath path)
        {
            return system.ObserverOf(path);
        }
    }
}