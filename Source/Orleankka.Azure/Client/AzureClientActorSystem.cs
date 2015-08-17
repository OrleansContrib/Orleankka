using System;

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
            AzureClient.Initialize(configuration);
        }

        public override void Dispose()
        {
            AzureClient.Uninitialize();
            configurator.Dispose();
        }
    }
}
