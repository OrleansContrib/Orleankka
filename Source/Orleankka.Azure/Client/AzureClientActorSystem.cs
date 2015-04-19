using System;
using System.Linq;

using Microsoft.WindowsAzure.ServiceRuntime;

using Orleans.Runtime.Host;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    class AzureClientActorSystem : ActorSystem
    {
        readonly IDisposable configurator;

        public AzureClientActorSystem(IDisposable configurator)
        {
            this.configurator = configurator;
        }

        public static void Initialize(ClientConfiguration configuration)
        {
            // TODO: make this configurable from outside

            configuration.DeploymentId = RoleEnvironment.DeploymentId;
            configuration.DataConnectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");
            configuration.GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable;

            AzureClient.Initialize(configuration);
        }

        public override void Dispose()
        {
            AzureClient.Uninitialize();
            configurator.Dispose();
        }
    }
}
