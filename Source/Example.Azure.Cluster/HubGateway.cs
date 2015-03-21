using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.ServiceRuntime;

using Orleankka;
using Orleankka.Cluster;

namespace Example.Azure
{
    public class HubGateway
    {
        static IActorSystem system;
        static IPEndPoint ip;

        public class Bootstrapper : Orleankka.Cluster.Bootstrapper
        {
            public override Task Run(IDictionary<string, string> properties)
            {
                system = ClusterActorSystem.Current;

                var instanceEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["OrleansSiloEndpoint"];
                ip = instanceEndpoint.IPEndpoint;

                var hub = GetLocalHub();
                return hub.Tell(new InitHub());
            }
        }

        public static Task Publish(Event e)
        {
            var buffer = system.ActorOf<HubBuffer>("any");
            return buffer.Tell(new PublishEvent{Event = e});
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
