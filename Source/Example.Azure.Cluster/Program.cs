using System.Collections.Generic;
using System.Net;
using System.Reflection;

using Microsoft.WindowsAzure.ServiceRuntime;

using Orleankka;
using Orleankka.Cluster;

using Orleans.Storage;
using Orleans.Runtime.Configuration;

namespace Example.Azure
{
    public class Program : RoleEntryPoint
    {
        static ClusterActorSystem system;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 100;
            return base.OnStart();
        }

        public override void Run()
        {
            var clusterId = RoleEnvironment.DeploymentId;
            var clusterMembershipStorage = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");

            system = ActorSystem.Configure()
                .Cluster()
                .From(Configuration(clusterId, clusterMembershipStorage))
                .Register(Assembly.GetExecutingAssembly())
                .Run<HubGateway.Bootstrapper>()
                .Done();

            system.Start(wait: true);
        }

        static ClusterConfiguration Configuration(string deploymentId, string dataConnectionString)
        {
            var cluster = new ClusterConfiguration()
                .LoadFromEmbeddedResource<Program>("Orleans.xml");

            cluster.Globals.DeploymentId = deploymentId;
            cluster.Globals.DataConnectionString = dataConnectionString;
            cluster.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;

            cluster.Globals.DataConnectionStringForReminders = dataConnectionString;
            cluster.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;

            cluster.Globals.RegisterStorageProvider<MemoryStorage>("MemoryStore");
            cluster.Globals.RegisterStorageProvider<AzureTableStorage>("PubSubStore", new Dictionary<string, string>
            {
                {"DataConnectionString", dataConnectionString}
            });

            return cluster;
        }
    }
}
