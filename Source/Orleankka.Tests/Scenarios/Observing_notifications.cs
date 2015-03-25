using System;
using System.Linq;
using System.Threading;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Testing;

    [TestFixture]
    [RequiresSilo]
    public class Observing_notifications
    {
        readonly IActorSystem system = TestActorSystem.Instance;

        [Test]
        public async void Client_to_actor()
        {
            var actor = system.FreshActorOf<TestActor>();

            using (var observer = await Observer.Create())
            {
                await actor.Tell(new Attach(observer));

                TextChanged @event = null;

                var done = new AutoResetEvent(false);
                var subscription = observer.Subscribe(notification =>
                {
                    @event = (TextChanged) notification;
                    done.Set();
                });

                await actor.Tell(new SetText("c-a"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(@event.Text, 
                    Is.EqualTo("c-a"));

                subscription.Dispose();

                await actor.Tell(new SetText("kaboom"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(@event.Text, Is.EqualTo("c-a"));
            }
        }
        
        [Test]
        public async void Actor_to_actor()
        {
            var actor = system.FreshActorOf<TestActor>();
            var observer = system.FreshActorOf<TestInsideActor>();

            await actor.Tell(new Attach(observer));
            await actor.Tell(new SetText("a-a"));

            var received = await observer.Ask<TextChanged[]>(new ReceivedNotifications());
            Assert.That(received.Length, Is.EqualTo(1));
            Assert.That(received[0].Text, Is.EqualTo("a-a"));

            await actor.Tell(new Detach(observer));
            await actor.Tell(new SetText("kaboom"));

            received = await observer.Ask<TextChanged[]>(new ReceivedNotifications());
            Assert.That(received.Length, Is.EqualTo(1), "Nothing new has been received");
        }
    }
}