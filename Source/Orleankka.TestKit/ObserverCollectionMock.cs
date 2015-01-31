using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.TestKit
{
    public class ObserverCollectionMock : IObserverCollection
    {
        public readonly List<object> RecordedNotifications = new List<object>();
        public readonly List<ObserverRef> RecordedSubscriptions = new List<ObserverRef>();

        void IObserverCollection.Notify(object message)
        {
            RecordedNotifications.Add(message);
        }

        void IObserverCollection.Add(ObserverRef observer)
        {
            if (RecordedSubscriptions.Any(x => x == observer))
                return;

            RecordedSubscriptions.Add(observer);
        }

        void IObserverCollection.Remove(ObserverRef observer)
        {
            RecordedSubscriptions.Remove(observer);
        }

        public IEnumerator<ObserverRef> GetEnumerator()
        {
            return RecordedSubscriptions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
