using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Observing_notifications
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Attach : Command
        {
            public ObserverRef Observer;
        }

        [Serializable]
        public class Publish : Command
        {
            public string Text;
        }

        [Serializable]
        public class Notification : Event
        {
            public string Text;
        }

        public class TestActor : Actor
        {
            ObserverRef observer;

            protected internal override void Define()
            {
                On((Attach x)   => observer = x.Observer);
                On((Publish x)  => observer.Notify(new Notification {Text = x.Text}));
            }
        }

        [Serializable]
        public class ReceivedNotifications : Query<Notification[]>
        {}

        public class TestInsideActor : Actor
        {
            readonly List<Notification> notifications = new List<Notification>();

            protected internal override void Define()
            {
                On((Notification x) => notifications.Add(x));
                On((ReceivedNotifications x) => notifications.ToArray());
            }
        }

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async void Client_to_actor()
            {
                var actor = system.FreshActorOf<TestActor>();

                using (var observer = await Observer.Create())
                {
                    await actor.Tell(new Attach {Observer = observer});

                    Notification @event = null;

                    var done = new AutoResetEvent(false);
                    var subscription = observer.Subscribe((Notification e) =>
                    {
                        @event = e;
                        done.Set();
                    });

                    await actor.Tell(new Publish {Text = "c-a"});
                    
                    done.WaitOne(TimeSpan.FromSeconds(5));
                    Assert.That(@event.Text, Is.EqualTo("c-a"));
                    
                    subscription.Dispose();
                    await actor.Tell(new Publish {Text = "kaboom"});
                    
                    done.WaitOne(TimeSpan.FromSeconds(5));
                    Assert.That(@event.Text, Is.EqualTo("c-a"));
                }
            }

            [Test]
            public async void Actor_to_actor()
            {
                var actor = system.FreshActorOf<TestActor>();
                var observer = system.FreshActorOf<TestInsideActor>();

                await actor.Tell(new Attach {Observer = observer});
                await actor.Tell(new Publish {Text = "a-a"});

                Notification[] received = await observer.Ask(new ReceivedNotifications());
                Assert.That(received.Length,  Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("a-a"));
            }
        }
    }
}
