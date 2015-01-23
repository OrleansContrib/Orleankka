using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ObserverCollectionMock : IObserverCollection
    {
        public readonly List<RecordedNotification> RecordedNotifications = new List<RecordedNotification>();
        public readonly List<RecordedSubscription> RecordedSubscriptions = new List<RecordedSubscription>();

        void IObserverCollection.Notify(string source, params Notification[] notifications)
        {
            RecordedNotifications.AddRange(notifications.Select(x => new RecordedNotification(source, x)));
        }

        void IObserverCollection.Attach(IObserve observer, Type notification)
        {
            if (RecordedSubscriptions.Any(x => x.Observer == observer && x.Notification == notification))
                return;

            RecordedSubscriptions.Add(new RecordedSubscription(observer, notification));
        }

        void IObserverCollection.Detach(IObserve observer, Type notification)
        {
            RecordedSubscriptions.RemoveAll(x => x.Observer == observer && x.Notification == notification);
        }
    }

    public class RecordedNotification
    {
        public readonly object Source;
        public readonly Notification Notification;

        public RecordedNotification(object source, Notification notification)
        {
            Source = source;
            Notification = notification;
        }
    }    
    
    public class RecordedSubscription
    {
        public readonly IObserve Observer;
        public readonly Type Notification;

        public RecordedSubscription(IObserve observer, Type notification)
        {
            Observer = observer;
            Notification = notification;
        }
    }
}
