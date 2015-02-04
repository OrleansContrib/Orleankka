using System;
using System.Linq;

namespace Orleankka.Embedded
{
    class EmbeddedActorSystem : IActorSystem
    {
        AppDomain domain;
        readonly IActorSystem client;
        readonly IActorSystem cluster;

        public EmbeddedActorSystem(AppDomain domain, IActorSystem client, IActorSystem cluster)
        {
            this.domain = domain;
            this.client = client;
            this.cluster = cluster;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return client.ActorOf(path);
        }

        public void Dispose()
        {
            if (domain == null)
                return;

            client.Dispose();
            cluster.Dispose();

            AppDomain.Unload(domain);
            domain = null;
        }
    }
}