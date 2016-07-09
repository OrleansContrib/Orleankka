using System;

using Orleankka.Client;
using Orleankka.Cluster;

namespace Orleankka.Embedded
{
    public class EmbeddedActorSystem : ActorSystem
    {
        AppDomain domain;
        readonly ClientActorSystem client;
        readonly ClusterActorSystem cluster;

        internal EmbeddedActorSystem(AppDomain domain, ClientActorSystem client, ClusterActorSystem cluster)
        {
            this.domain = domain;
            this.client = client;
            this.cluster = cluster;
        }

        public ClientActorSystem Client => client;
        public ClusterActorSystem Cluster => cluster;

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