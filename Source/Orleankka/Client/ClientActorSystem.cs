using System;

using Orleans;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    sealed class ClientActorSystem : ActorSystem
    {
        static IActorSystem current;

        public static IActorSystem Current
        {
            get
            {
                if (!Initialized)
                    throw new InvalidOperationException("Client actor system hasn't been initialized");

                return current;
            }

            internal set
            {
                current = value;
            }
        }

        public static bool Initialized => current != null;

        readonly IDisposable configurator;

        public ClientActorSystem(IDisposable configurator)
        {
            current = this;
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
            current = null;
        }
    }
}