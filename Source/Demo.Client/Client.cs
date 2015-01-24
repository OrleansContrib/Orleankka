using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Demo
{
    public class Client
    {
        readonly IActorSystem system;
        readonly IClientObservable observer;

        public Client(IActorSystem system, IClientObservable observer)
        {
            this.system = system;
            this.observer = observer;
        }

        public async void Run()
        {
            await MonitorAvailabilityChanges("facebook");
            await MonitorAvailabilityChanges("twitter");

            observer.Subscribe(LogToConsole);

            foreach (var i in Enumerable.Range(1, 25))
            {
                var topic = system.ActorOf<ITopic>(i.ToString());

                await topic.Send(new CreateTopic("[" + i + "]", new Dictionary<string, TimeSpan>
                {
                    {"facebook", TimeSpan.FromMinutes(1)},
                    {"twitter", TimeSpan.FromMinutes(1)},
                }));
            }
        }

        async Task MonitorAvailabilityChanges(string api)
        {
            await system.ActorOf<IApi>(api).Tell(new MonitorAvailabilityChanges(observer));
        }

        static void LogToConsole(Notification notification)
        {
            var e = (AvailabilityChanged) notification.Message;

            Log.Message(
                !e.Available ? ConsoleColor.Red : ConsoleColor.Green,
                !e.Available ? "*{0}* gone wild. Unavailable!" : "*{0}* is back available again!", 
                notification.Source.Id);
        }
    }
}
