﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Observing_notifications
    {
        using Meta;
        using Client;
        using Testing;
        using Orleans.Metadata;

        [Serializable]
        public class Attach : Command
        {
            public ObserverRef Observer;
        }

        [Serializable]
        public class AttachClientViaPath : Command
        {
            public string ClientPath;
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

        [DefaultGrainType("observer-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        [GrainType("observer-test")]
        public class TestActor : DispatchActorGrain, ITestActor
        {
            ObserverRef observer;

            void On(Attach x) => observer = x.Observer;
            void On(AttachClientViaPath x) => observer = System.ClientOf(x.ClientPath);
            void On(Publish x) => observer.Notify(new Notification {Text = x.Text});
        }

        [Serializable]
        public class ReceivedNotifications : Query<Notification[]>
        {}

        [DefaultGrainType("inside-observer-test")]
        public interface ITestInsideActor : IActorGrain, IGrainWithStringKey
        {}

        [GrainType("inside-observer-test")]
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
                await CheckClientRef(x => new Attach {Observer = x.Ref});
            }
            
            [Test]
            public async Task Client_to_actor_via_path()
            {
                await CheckClientRef(x => new AttachClientViaPath {ClientPath = x.Ref});
            }

            async Task CheckClientRef(Func<IClientObservable, object> buildMessage)
            {
                var actor = system.FreshActorOf<ITestActor>();

                using (var observable = system.CreateObservable())
                {
                    await actor.Tell(buildMessage(observable));

                    Notification @event = null;

                    var done = new AutoResetEvent(false);
                    var subscription = observable.Subscribe((Notification e) =>
                    {
                        @event = e;
                        done.Set();
                    });

                    await actor.Tell(new Publish {Text = "c-a"});

                    done.WaitOne(TimeSpan.FromSeconds(5));
                    Assert.NotNull(@event);
                    Assert.That(@event.Text, Is.EqualTo("c-a"));

                    @event = null;
                    subscription.Dispose();
                    await actor.Tell(new Publish {Text = "kaboom"});

                    done.WaitOne(TimeSpan.FromSeconds(1));
                    Assert.Null(@event);
                }
            }

            [Test]
            public async Task Actor_to_actor()
            {
                var actor = system.FreshActorOf<ITestActor>();
                var observer = system.FreshActorOf<ITestInsideActor>();

                await actor.Tell(new Attach {Observer = observer});
                await actor.Tell(new Publish {Text = "a-a"});

                await Task.Delay(500);

                var received = await observer.Ask(new ReceivedNotifications());
                Assert.That(received.Length,  Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("a-a"));
            }
        }
    }
}
