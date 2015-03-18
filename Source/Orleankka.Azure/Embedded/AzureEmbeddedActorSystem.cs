using System;
using System.Linq;

namespace Orleankka.Embedded
{
    using Cluster;

    public class AzureEmbeddedActorSystem : EmbeddedActorSystem
    {
        readonly AzureClusterActorSystem cluster;

        internal AzureEmbeddedActorSystem(AppDomain domain, IActorSystem client, AzureClusterActorSystem cluster)
            : base(domain, client, cluster)
        {
            this.cluster = cluster;
        }

        public void Run()
        {
            cluster.Run();
        }
    }
}
