using System;
using System.Linq;

using Orleans;

namespace Orleankka.Client
{
    public sealed class ClientActorSystem : IActorSystem
    {
        readonly IActorSystemConfigurator configurator;

        public ClientActorSystem(IActorSystemConfigurator configurator)
        {
            this.configurator = configurator;
        }

        ActorRef IActorSystem.ActorOf(ActorPath path)
        {
            return ActorRef.Resolve(path);
        }

        public void Dispose()
        {
            GrainClient.Uninitialize();
            configurator.Dispose();
        }
    }
}