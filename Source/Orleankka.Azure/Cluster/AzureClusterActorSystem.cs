using Orleans.Runtime.Host;

namespace Orleankka.Cluster
{
    public class AzureClusterActorSystem : ActorSystem
    {
        readonly ClusterConfigurator cluster;
        readonly string deploymentId;
        readonly string connectionString;

        AzureSilo host;

        internal AzureClusterActorSystem(ClusterConfigurator cluster, string deploymentId, string connectionString)
        {
            ClusterActorSystem.Current = this;

            this.cluster = cluster;
            this.deploymentId = deploymentId;
            this.connectionString = connectionString;

            host = new AzureSilo();
        }

        internal void Start()
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
