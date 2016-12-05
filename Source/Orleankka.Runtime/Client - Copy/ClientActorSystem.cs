using System;
using System.Diagnostics;
using System.Threading;

using Orleans;
using Orleans.Runtime;
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

        public void Connect(int retries = 0)
        {
            if (retries < 0)
                throw new ArgumentOutOfRangeException(nameof(retries), 
                    "retries should be greater than or equal to 0");

            while (retries-- >= 0)
            {
                try
                {
                    GrainClient.Initialize(configuration);
                    return;
                }
                catch (SiloUnavailableException)
                {
                    if (retries >= 0)
                    {
                        Trace.TraceWarning("Can't connect to cluster. Trying again ...");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                    else
                    {
                        Trace.TraceError("Can't connect to cluster. Max retries reached");
                        throw;
                    }
                }
            }
        }

        public void Reconnect(string deploymentId = null, int retries = 0)
        {
            var clusterId = deploymentId ?? configuration.DeploymentId;

            Trace.TraceInformation("Reconnecting to cluster with DeploymentId '{0}' ...", clusterId);
            configuration.DeploymentId = clusterId;

            Reset();
            Connect(retries);
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