using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Fixtures
{
    [TestFixture]
    public class ObserverCollectionFixture
    {
        readonly IActorSystem system = ActorSystem.Instance;
        IObserverCollection collection;
        ActorRef @ref;

        [SetUp]
        public void SetUp()
        {
            @ref = system.ActorOf<TestObservableActor>("producer");
            collection = new ObserverCollection(()=> @ref);
        }

        [Test]
        public void Notify_when_no_observers()
        {
            Assert.DoesNotThrow(() => collection.Notify("foo"));
        }

        [Test]
        public async void Creates_notification_with_passed_in_source_actor_path()
        {
            var observer = await TestObserver.Create();
            
            collection.Add(observer);
            collection.Notify("foo");

            observer.Received.WaitOne(TimeSpan.FromSeconds(5));
            Assert.That(observer.Notifications.Count, 
                Is.EqualTo(1));

            var notification = observer.Notifications[0];
            
            Assert.That(notification.Sender, Is.EqualTo(@ref));
            Assert.That(notification.Message, Is.EqualTo("foo"));
        }
        
        [Test]
        public async void Add_is_idempotent()
        {
            var observer = await TestObserver.Create();
            collection.Add(observer);

            Assert.DoesNotThrow(() => collection.Add(observer));
            Assert.AreEqual(1, collection.Count());
        }

        [Test]
        public async void Remove_is_also_idempotent()
        {
            var observer = await TestObserver.Create();
            
            collection.Add(observer);
            collection.Remove(observer);

            Assert.DoesNotThrow(() => collection.Remove(observer));
            Assert.AreEqual(0, collection.Count());
        }

        class TestObservableActor : Actor
        {}

        class TestObserver
        {
            public static async Task<TestObserver> Create()
            {
                var observer = await Observer.Create();
                return new TestObserver(observer);
            }

            public readonly List<Notification> Notifications = new List<Notification>();
            public readonly EventWaitHandle Received = new AutoResetEvent(false);
            readonly Observer observer;

            TestObserver(Observer observer)
            {
                this.observer = observer;
                observer.Subscribe(notification =>
                {
                    Notifications.Add(notification);
                    Received.Set();
                });
            }

            public static implicit operator ObserverRef(TestObserver arg)
            {
                return arg.observer;
            }
        }
    }
}
