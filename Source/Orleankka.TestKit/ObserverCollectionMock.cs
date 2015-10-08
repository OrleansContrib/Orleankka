using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Orleankka.TestKit
{
    public class ObserverCollectionMock : IObserverCollection
    {
        readonly List<object> messages = new List<object>();
        readonly List<ObserverRef> subscriptions = new List<ObserverRef>();

        public IEnumerable<object> RecordedMessages => messages;
        public IEnumerable<ObserverRef> RecordedSubscriptions => subscriptions;

        void IObserverCollection.Notify(object message)
        {
            messages.Add(message);
        }

        void IObserverCollection.Add(ObserverRef observer)
        {
            if (subscriptions.Any(x => x == observer))
                return;

            subscriptions.Add(observer);
        }

        void IObserverCollection.Remove(ObserverRef observer)
        {
            subscriptions.Remove(observer);
        }

        public IEnumerator<ObserverRef> GetEnumerator() => subscriptions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Reset()
        {
            messages.Clear();
            subscriptions.Clear();
        }
    }
}
