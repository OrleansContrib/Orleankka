using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using Orleankka;

using Orleans;
using Orleans.Runtime;

namespace Example.Azure.Hubs
{
    public class Relay : Microsoft.AspNet.SignalR.Hub
    {}

    public class HubClient
    {
        static ClientObservable notifications;
        static IHubConnectionContext<dynamic> clients;

        public static void Initialize()
        {
            clients = GlobalHost.ConnectionManager.GetHubContext<Relay>().Clients;
            
            notifications = Task.Run(ClientObservable.Create).Result;
            notifications.Subscribe(On);

            Task.Run(Subscribe)
                .Wait();

            Task.Run(Resubscribe);
        }

        static async Task Subscribe()
        {
            var orleans = GrainClient.GrainFactory.GetGrain<IManagementGrain>(0);
            var hosts = await orleans.GetHosts(true);

            foreach (var silo in hosts)
            {
                var address = silo.Key;
                var status = silo.Value;

                if (status != SiloStatus.Active)
                    continue;

                var id = HubGateway.HubId(address.Endpoint);
                var hub = MvcApplication.System.ActorOf<Hub>(id);

                await hub.Tell(new Hub.Subscribe {Observer = notifications});
            }
        }

        static async Task Resubscribe()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(120));
                await Subscribe();
            }
        }

        static void On(object message)
        {
            var messages = (IEnumerable<Notification>) message;
            clients.All.receive(messages.Select(Format));
        }

        static string Format(Notification notification)
        {
            var @event = notification.Event;
            var latency = (DateTime.Now - notification.Received).TotalSeconds;

            return string.Format("{0} published {1} on {2} via {4}. Latency: {3} s",
                                 @event.Sender, @event.Id, @event.Time, latency, notification.Hub);
        }
    }
}