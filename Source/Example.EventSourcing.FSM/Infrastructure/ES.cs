using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using EventStore.ClientAPI;

using Orleankka;
using Orleankka.Cluster;

namespace FSM.Infrastructure
{
    public static class ES
    {
        public static IEventStoreConnection Connection { get; private set; }

        public class Bootstrap : IBootstrapper
        {
            public async Task Run(IActorSystem system, object properties)
            {
                Connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
                await Connection.ConnectAsync();
            }
        }
    }
}