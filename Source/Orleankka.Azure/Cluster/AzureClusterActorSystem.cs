using System;
using System.Linq;

using Orleans.Runtime.Host;

namespace Orleankka.Cluster
{
    public class AzureClusterActorSystem : ActorSystem
    {
        readonly ClusterConfigurator cluster;
        AzureSilo host;

        internal AzureClusterActorSystem(ClusterConfigurator cluster)
        {
            ClusterActorSystem.Current = this;
            this.cluster = cluster;
            host = new AzureSilo();
        }

        internal void Start(string deploymentId = null, string connectionString = null)
        {
            host.Start(cluster.Configuration, deploymentId, connectionString);
        }

        public void Run()
        {
            host.Run();
        }

        public override void Dispose()
        {
            if (host == null)
                return;

            host.Stop();
            host = null;

            cluster.Dispose();
        }
    }
}
