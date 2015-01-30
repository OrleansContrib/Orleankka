using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NUnit.Framework;
using Orleankka.Core;

namespace Orleankka.Fixtures
{
    [TestFixture]
    public class ObserverCollectionFixture
    {
        const string message = "foo";
        static readonly ObserverPath source = ObserverPath.From("some-id");

        IObserverCollection collection;
        ActorObserver observer;
        IObserverEndpoint proxy;

        [SetUp]
        public void SetUp()
        {
            observer = new ActorObserver();
            proxy = ObserverEndpointFactory.CreateObjectReference(observer).Result;
            collection = new ObserverCollection(()=> source);
        }

        [Test]
        public void Notify_when_no_observers()
        {
            Assert.DoesNotThrow(() => collection.Notify(message));
        }

        [Test]
        public void Creates_notification_with_passed_in_source_actor_path()
        {
            collection.Add(observer);
            collection.Notify(message);

            observer.Received.WaitOne(TimeSpan.FromSeconds(5));
            Assert.That(observer.Notifications.Count, 
                Is.EqualTo(1));

            var notification = observer.Notifications[0];
            
            Assert.That(notification.Sender, Is.EqualTo(source));
            Assert.That(notification.Message, Is.EqualTo(message));
        }
        
        [Test]
        public void Add_is_idempotent()
        {
            collection.Add(observer);

            Assert.DoesNotThrow(() => collection.Add(observer));
            Assert.AreEqual(1, collection.Count());
        }

        [Test]
        public void Remove_is_also_idempotent()
        {
            collection.Add(observer);
            collection.Remove(observer);

            Assert.DoesNotThrow(() => collection.Remove(observer));
            Assert.AreEqual(0, collection.Count());
        }

        class ActorObserver : IObserverEndpoint
        {
            public readonly List<Notification> Notifications = new List<Notification>();
            public readonly EventWaitHandle Received = new AutoResetEvent(false);
            
            public void ReceiveNotify(NotificationEnvelope envelope)
            {
                Notifications.Add(new Notification(envelope.Message);
                Received.Set();
            }
        }
    }
}
