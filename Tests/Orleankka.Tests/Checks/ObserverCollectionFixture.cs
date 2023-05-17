using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Checks
{
    using Testing;

    [TestFixture]
    [RequiresSilo]
    public class ObserverCollectionFixture
    {
        IObserverCollection collection;

        [SetUp]
        public void SetUp()
        {
            collection = new ObserverCollection();
        }

        [Test]
        public void Notify_when_no_observers()
        {
            Assert.DoesNotThrow(() => collection.Notify("foo"));
        }

        [Test]
        public void Notifies_all_observers()
        {
            var observer1 = TestObserver.Create();
            var observer2 = TestObserver.Create();
            
            collection.Add(observer1);
            collection.Add(observer2);
            collection.Notify("foo");

            observer1.Received.WaitOne(TimeSpan.FromSeconds(1));
            observer2.Received.WaitOne(TimeSpan.FromSeconds(1));
            
            Assert.That(observer1.Notifications.Count, Is.EqualTo(1));
            Assert.That(observer1.Notifications[0], Is.EqualTo("foo"));

            Assert.That(observer2.Notifications.Count, Is.EqualTo(1));
            Assert.That(observer2.Notifications[0], Is.EqualTo("foo"));
        }
        
        [Test]
        public void Add_is_idempotent()
        {
            var observer = TestObserver.Create();
            collection.Add(observer);

            Assert.DoesNotThrow(() => collection.Add(observer));
            Assert.AreEqual(1, collection.Count());
        }

        [Test]
        public void Remove_is_also_idempotent()
        {
            var observer = TestObserver.Create();
            
            collection.Add(observer);
            collection.Remove(observer);

            Assert.DoesNotThrow(() => collection.Remove(observer));
            Assert.AreEqual(0, collection.Count());
        }

        interface ITestObservableActor : IActorGrain, IGrainWithStringKey
        {}

        class TestObservableActor : DispatchActorGrain, ITestObservableActor
        {}

        class TestObserver
        {
            public static TestObserver Create()
            {
                var observable = TestActorSystem.Instance.CreateObservable();
                return new TestObserver(observable);
            }

            public readonly List<object> Notifications = new List<object>();
            public readonly EventWaitHandle Received = new AutoResetEvent(false);
            readonly IClientObservable observable;

            TestObserver(IClientObservable observable)
            {
                this.observable = observable;
                observable.Subscribe(message =>
                {
                    Notifications.Add(message);
                    Received.Set();
                });
            }

            public static implicit operator ObserverRef(TestObserver arg) => arg.observable.Ref;
        }
    }
}
