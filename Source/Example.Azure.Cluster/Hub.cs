using System;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Azure
{
    [Actor(Placement = Placement.PreferLocal)]
    public class Hub : Actor
    {
        [Serializable]
        public class Init : Command 
        {}

        [Serializable]
        public class Subscribe : Command
        {
            public ObserverRef Observer;
        }

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

        protected override void Define()
        {
            On((Init x) => {});
            
            On((Subscribe x) => observers.Add(x.Observer));
            
            On((Publish x) =>
            {
                var notifications = x.Events
                    .Select(e => new Notification(e, DateTime.Now, HubGateway.LocalHubId()))
                    .ToArray();

                observers.Notify(notifications);                
            });
        }
    }
}