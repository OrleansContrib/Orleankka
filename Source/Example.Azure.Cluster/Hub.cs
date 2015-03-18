using System;
using System.Linq;

using Orleankka;

namespace Example.Azure
{
    [Serializable]
    public class InitHub {}

    [Serializable]
    public class Subscribe
    {
        public ObserverRef Observer;
    }

    [Serializable]
    public class PublishEvents
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

        public void Handle(InitHub req)
        {}

        public void Handle(Subscribe req)
        {
            observers.Add(req.Observer);
        }
        
        public void Handle(PublishEvents req)
        {
            var notifications = req
                .Events
                .Select(e => new Notification(e, DateTime.Now, HubGateway.LocalHubId()))
                .ToArray();

            observers.Notify(notifications);
        }
    }
}