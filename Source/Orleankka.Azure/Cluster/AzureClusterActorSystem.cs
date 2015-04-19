using System;
using System.Linq;

using Microsoft.WindowsAzure.ServiceRuntime;
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

        internal void Start()
        {
            host.Start(RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance, cluster.Configuration);
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
