using System;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Azure
{
    [Serializable]
    public class InitHub : Command 
    {}

    [Serializable]
    public class Subscribe : Command
    {
        public ObserverRef Observer;
    }

    [Serializable]
    public class PublishEvents : Command
    {
        public Event[] Events;
    }

    public class Hub : Actor
    {
        readonly IObserverCollection observers;

        public Hub()
        {
            observers = new ObserverCollection();
        }

        protected override void Define()
        {
            On((InitHub x) => {});
            
            On((Subscribe x) => observers.Add(x.Observer));
            
            On((PublishEvents x) =>
            {
                var notifications = x.Events
                    .Select(e => new Notification(e, DateTime.Now, HubGateway.LocalHubId()))
                    .ToArray();

                observers.Notify(notifications);                
            });
        }
    }
}