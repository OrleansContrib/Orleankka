using System;

using Orleans;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    public sealed class ClientActorSystem : ActorSystem
    {
        static ClientActorSystem current;

        internal static ClientActorSystem Current
        {
            get
            {
                if (!Initialized)
                    throw new InvalidOperationException("Client actor system hasn't been initialized");

                return current;
            }
        }

        internal static bool Initialized => current != null;

        readonly IDisposable configurator;

        internal ClientActorSystem(IDisposable configurator)
        {
            current = this;
            this.configurator = configurator;
        }

        internal void Initialize(ClientConfiguration configuration)
        {
            GrainClient.Initialize(configuration);
        }

        public override void Dispose()
        {
            GrainClient.Uninitialize();
            configurator.Dispose();
            current = null;
        }
    }
}