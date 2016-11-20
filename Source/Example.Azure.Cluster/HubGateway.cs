using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Cluster;

namespace Example.Azure
{
    public class HubGateway
    {
        static ClusterActorSystem system;
        static IPEndPoint ip;

        public class Bootstrapper : IBootstrapper
        {
            public Task Run(ClusterActorSystem system, object properties)
            {
                HubGateway.system = system;

                ip = system.Silo.SiloAddress.Endpoint;
                Trace.TraceError(ip.ToString());

                var hub = GetLocalHub();
                return hub.Tell(new Hub.Init());
            }
        }

        public static Task Publish(Event e)
        {
            var buffer = system.ActorOf<HubBuffer>("any");
            return buffer.Tell(new HubBuffer.Publish {Event = e});
        }

        static ActorRef GetHub(IPEndPoint endpoint)
        {
            return system.ActorOf<Hub>(HubId(endpoint));
        }

        public static ActorRef GetLocalHub()
        {
            return GetHub(ip);
        }

        public static string HubId(IPEndPoint endpoint)
        {
            Debug.Assert(endpoint != null);
            return "HUB" + endpoint.Address;
        }

        public static string LocalHubId()
        {
            return HubId(ip);
        }

        public static IPAddress LocalAddress()
        {
            return ip.Address;
        }
    }
}
