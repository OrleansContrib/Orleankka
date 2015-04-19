using System;
using System.Linq;

namespace Orleankka.Embedded
{
    public class EmbeddedActorSystem : ActorSystem
    {
        AppDomain domain;
        readonly IActorSystem client;
        readonly IActorSystem cluster;

        internal EmbeddedActorSystem(AppDomain domain, IActorSystem client, IActorSystem cluster)
        {
            this.domain = domain;
            this.client = client;
            this.cluster = cluster;
        }

        public override void Dispose()
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