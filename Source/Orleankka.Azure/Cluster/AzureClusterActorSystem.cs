using System;
using System.Linq;

using Microsoft.WindowsAzure.ServiceRuntime;

using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace Orleankka.Cluster
{
    public class AzureClusterActorSystem : MarshalByRefObject, IActorSystem
    {
        readonly IDisposable configurator;
        readonly ClusterConfiguration configuration;
        AzureSilo host;

        internal AzureClusterActorSystem(IDisposable configurator, ClusterConfiguration configuration)
        {
            this.configurator = configurator;
            this.configuration = configuration;

            host = new AzureSilo();
            ClusterActorSystem.Current = this;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return ActorRef.Resolve(path);
        }

        internal void Start()
        {
            host.Start(RoleEnvironment.DeploymentId, RoleEnvironment.CurrentRoleInstance, configuration);
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

            configurator.Dispose();
        }
    }
}
