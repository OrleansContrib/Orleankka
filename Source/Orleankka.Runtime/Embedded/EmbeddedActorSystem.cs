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

        public async Task Start()
        {
            await Cluster.Start();
            await Client.Connect(); 
        }

        public async Task Stop()
        {
            await Client.Disconnect();
            await Cluster.Stop();
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