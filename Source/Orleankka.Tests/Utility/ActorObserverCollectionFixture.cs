using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NUnit.Framework;

namespace Orleankka
{
    [TestFixture]
    public class ActorObserverCollectionFixture
    {
        const string message = "foo";
        static readonly ActorPath source = new ActorPath(typeof(ITestActor), "some-id");

        IActorObserverCollection collection;
        ActorObserver observer;
        IActorObserver proxy;

        [SetUp]
        public void SetUp()
        {
            observer = new ActorObserver();
            proxy = ActorObserverFactory.CreateObjectReference(observer).Result;
            collection = new ActorObserverCollection(()=> source);
        }

        [Test]
        public void Notify_when_no_observers()
        {
            Assert.DoesNotThrow(() => collection.Notify(message));
        }

        [Test]
        public void Creates_notification_with_passed_in_source_actor_path()
        {
            collection.Add(proxy);
            collection.Notify(message);

            observer.Received.WaitOne(TimeSpan.FromSeconds(5));
            Assert.That(observer.Notifications.Count, 
                Is.EqualTo(1));

            var notification = observer.Notifications[0];
            
            Assert.That(notification.Source, Is.EqualTo(source));
            Assert.That(notification.Message, Is.EqualTo(message));
        }
        
        [Test]
        public void Add_is_idempotent()
        {
            collection.Add(proxy);

            Assert.DoesNotThrow(() => collection.Add(proxy));
            Assert.AreEqual(1, collection.Count());
        }

        [Test]
        public void Remove_is_also_idempotent()
        {
            collection.Add(proxy);
            collection.Remove(proxy);

            Assert.DoesNotThrow(() => collection.Remove(proxy));
            Assert.AreEqual(0, collection.Count());
        }

        class ActorObserver : IActorObserver
        {
            public readonly List<Notification> Notifications = new List<Notification>();
            public readonly EventWaitHandle Received = new AutoResetEvent(false);
            
            public void OnNext(Notification notification)
            {
                Notifications.Add(notification);
                Received.Set();
            }
        }
    }

}
