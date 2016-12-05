using System;

using Orleankka.Client;
using Orleankka.Cluster;

namespace Orleankka.Embedded
{
    public class EmbeddedActorSystem : ActorSystem
    {
        AppDomain domain;
        readonly ClientActorSystem client;
        readonly ClusterActorSystem cluster;

        internal EmbeddedActorSystem(AppDomain domain, ClientActorSystem client, ClusterActorSystem cluster)
        {
            this.domain = domain;
            this.client = client;
            this.cluster = cluster;
        }

        public ClientActorSystem Client => client;
        public ClusterActorSystem Cluster => cluster;

        public void Start(bool wait = false)
        {
            cluster.Start();
            client.Connect(); 

            if (wait)
                cluster.Host.WaitForOrleansSiloShutdown();
        }

        public void Stop(bool force = false)
        {
            client.Disconnect();
            cluster.Stop(force);
        }

        public override void Dispose()
        {
            if (domain == null)
                return;

            client.Dispose();
            cluster.Dispose();

            AppDomain.Unload(domain);
            domain = null;
        }
    }
}