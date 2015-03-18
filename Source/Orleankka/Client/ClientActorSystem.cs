using System;
using System.Linq;

using Orleans;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    sealed class ClientActorSystem : IActorSystem
    {
        readonly IDisposable configurator;

        public ClientActorSystem(IDisposable configurator)
        {
            this.configurator = configurator;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return ActorRef.Resolve(path);
        }

        public static void Initialize(ClientConfiguration configuration)
        {
            GrainClient.Initialize(configuration);
        }

        public void Dispose()
        {
            GrainClient.Uninitialize();
            configurator.Dispose();
        }
    }
}