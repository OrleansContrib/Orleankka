using System;
using System.Linq;

using Orleans;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    sealed class ClientActorSystem : ActorSystem
    {
        readonly IDisposable configurator;

        public ClientActorSystem(IDisposable configurator)
        {
            this.configurator = configurator;
        }

        public static void Initialize(ClientConfiguration configuration)
        {
            GrainClient.Initialize(configuration);
        }

        public override void Dispose()
        {
            GrainClient.Uninitialize();
            configurator.Dispose();
        }
    }
}