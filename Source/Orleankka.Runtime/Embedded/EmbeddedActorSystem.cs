using System;
using System.Threading.Tasks;

namespace Orleankka.Embedded
{
    using Client;
    using Cluster;

    public class EmbeddedActorSystem : IClientActorSystem, IDisposable
    {
        internal EmbeddedActorSystem(ClientActorSystem client, ClusterActorSystem cluster)
        {
            Client = client;
            Cluster = cluster;
        }

        public ClientActorSystem Client { get; }
        public ClusterActorSystem Cluster { get; }

        public async Task Start(bool wait = false)
        {
            Cluster.Start();
            await Client.Connect(); 

            if (wait)
                Cluster.Host.WaitForOrleansSiloShutdown();
        }

        public async Task Stop(bool force = false)
        {
            Cluster.Stop(force);
            await Client.Disconnect(force);
        }

        public ActorRef ActorOf(ActorPath path) => Client.ActorOf(path);
        public StreamRef StreamOf(StreamPath path) => Client.StreamOf(path);
        public ClientRef ClientOf(string path) => Client.ClientOf(path);
        public Task<IClientObservable> CreateObservable() => Client.CreateObservable();

        public void Dispose()
        {
            Client?.Dispose();
            Cluster?.Dispose();
        }
    }
}