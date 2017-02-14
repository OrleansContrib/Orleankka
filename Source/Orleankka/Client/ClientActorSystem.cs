using System;
using System.Diagnostics;
using System.Threading;

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
        readonly ClientConfiguration configuration;

        internal ClientActorSystem(IDisposable configurator, ClientConfiguration configuration)
        {
            current = this;
            this.configurator = configurator;
            this.configuration = configuration;
        }

        public bool Connected => GrainClient.IsInitialized;

        public void Connect(int retries = 0, TimeSpan? retryTimeout = null)
        {
            if (retryTimeout == null)
                retryTimeout = TimeSpan.FromSeconds(5);

            if (retries < 0)
                throw new ArgumentOutOfRangeException(nameof(retries), 
                    "retries should be greater than or equal to 0");

            while (!Connected && retries-- >= 0)
            {
                try
                {
                    GrainClient.Initialize(configuration);
                }
                catch (Exception ex)
                {
                    if (retries >= 0)
                    {
                        Trace.TraceWarning($"Can't connect to cluster. Trying again in {(int)retryTimeout.Value.TotalSeconds} seconds ... Got error: /n{ex}");
                        Thread.Sleep(retryTimeout.Value);
                    }
                    else
                    {
                        Trace.TraceError($"Can't connect to cluster. Max retries reached. Got error: /n{ex}");
                        throw;
                    }
                }
            }
        }

        public void Disconnect()
        {
            Reset();
            current = null;
        }

        public override void Dispose()
        {
            Disconnect();
            configurator.Dispose();
        }

        static void Reset()
        {
            if (GrainClient.IsInitialized)
                GrainClient.Uninitialize();
        }
    }
}