using System;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

using Orleans.Placement;

namespace Example.Azure
{
    [PreferLocalPlacement]
    public class Hub : Actor, IHub
    {
        [Serializable]
        public class Init : Command
        {}

        [Serializable]
        public class Publish : Command
        {
            public Event[] Events;
        }

        readonly IObserverCollection observers;

        public Hub()
        {
            observers = new ObserverCollection();
        }

        public void On(Init _) {}

        public void On(SubscribeHub x) => observers.Add(x.Observer);

        public void On(Publish x)
        {
            var notifications = x.Events
                .Select(e => new Notification(e, DateTime.Now, HubGateway.LocalHubId()))
                .ToArray();

            observers.Notify(notifications);                
        }
    }
}