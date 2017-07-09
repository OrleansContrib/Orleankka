namespace Orleankka.Embedded
{
    using Client;
    using Cluster;

    public class EmbeddedActorSystem : IActorSystem
    {
        internal EmbeddedActorSystem(ClientActorSystem client, ClusterActorSystem cluster)
        {
            Client = client;
            Cluster = cluster;
        }

        public ClientActorSystem Client { get; }
        public ClusterActorSystem Cluster { get; }

        public void Start(bool wait = false)
        {
            Cluster.Start();
            Client.Connect(); 

            if (wait)
                Cluster.Host.WaitForOrleansSiloShutdown();
        }

        public void Stop(bool force = false)
        {
            Cluster.Stop(force);
        }

        public ActorRef ActorOf(ActorPath path) => Client.ActorOf(path);
        public StreamRef StreamOf(StreamPath path) => Client.StreamOf(path);
    }
}