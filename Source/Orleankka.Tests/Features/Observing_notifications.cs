using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Observing_notifications
    {
        using Meta;
        using Client;
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

        public interface ITestActor : IActorGrain
        {}

        public class TestActor : DispatchActorGrain, ITestActor
        {
            ObserverRef observer;

            void On(Attach x)   => observer = x.Observer;
            void On(Publish x)  => observer.Notify(new Notification {Text = x.Text});
        }

        [Serializable]
        public class ReceivedNotifications : Query<Notification[]>
        {}

        public interface ITestInsideActor : IActorGrain
        {}

        public class TestInsideActor : DispatchActorGrain, ITestInsideActor
        {
            readonly List<Notification> notifications = new List<Notification>();

            void On(Notification x)                    => notifications.Add(x);
            Notification[] On(ReceivedNotifications x) => notifications.ToArray();
        }

        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            IClientActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async Task Client_to_actor()
            {
                var actor = system.FreshActorOf<TestActor>();

                using (var observable = await system.CreateObservable())
                {
                    await actor.Tell(new Attach {Observer = observable.Ref});

                    Notification @event = null;

                    var done = new AutoResetEvent(false);
                    var subscription = observable.Subscribe((Notification e) =>
                    {
                        @event = e;
                        done.Set();
                    });

                    await actor.Tell(new Publish {Text = "c-a"});
                    
                    done.WaitOne(TimeSpan.FromMilliseconds(100));
                    Assert.That(@event.Text, Is.EqualTo("c-a"));
                    
                    subscription.Dispose();
                    await actor.Tell(new Publish {Text = "kaboom"});
                    
                    done.WaitOne(TimeSpan.FromMilliseconds(100));
                    Assert.That(@event.Text, Is.EqualTo("c-a"));
                }
            }

            [Test]
            public async Task Actor_to_actor()
            {
                var actor = system.FreshActorOf<TestActor>();
                var observer = system.FreshActorOf<TestInsideActor>();

                await actor.Tell(new Attach {Observer = observer});
                await actor.Tell(new Publish {Text = "a-a"});

                var received = await observer.Ask(new ReceivedNotifications());
                Assert.That(received.Length,  Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("a-a"));
            }
        }
    }
}
