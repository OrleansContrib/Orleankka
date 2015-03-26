using System;
using System.Linq;

using Microsoft.WindowsAzure.ServiceRuntime;
using Orleans.Runtime.Host;

namespace Orleankka.Cluster
{
    public class AzureClusterActorSystem : MarshalByRefObject, IActorSystem
    {
        readonly ClusterConfigurator cluster;
        AzureSilo host;

        internal AzureClusterActorSystem(ClusterConfigurator cluster)
        {
            this.cluster = cluster;
            host = new AzureSilo();
            
            ClusterActorSystem.Current = this;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return ActorRef.Resolve(path);
        }

        internal void Start()
        {
            host.Start(RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance, cluster.Configuration);
        }

        public void Run()
        {
            host.Run();
        }

        public void Dispose()
        {
            if (host == null)
                return;

            host.Stop();
            host = null;

            cluster.Dispose();
        }
    }
}
