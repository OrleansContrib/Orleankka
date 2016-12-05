using System;
using System.Linq;

using Orleankka;
using Orleankka.Meta;

namespace Example.Azure
{
    namespace Hub
    {
        class Init : Command {}

        class Subscribe : Command
        {
            public ObserverRef Observer;
        }

        class Publish : Command
        {
            public Event[] Events;
        }

        [Actor(Placement = Placement.PreferLocal)]
        class Actor : Orleankka.Actor
        {
            readonly IObserverCollection observers = new ObserverCollection();

            void On(Init x) {}

            void On(Subscribe x) => observers.Add(x.Observer);

            void On(Publish x)
            {
                var notifications = x.Events
                    .Select(e => new Notification(e, DateTime.Now, HubGateway.LocalHubId()))
                    .ToArray();

                observers.Notify(notifications);
            }
        }
    }
}