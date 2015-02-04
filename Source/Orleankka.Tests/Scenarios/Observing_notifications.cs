using System;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Orleankka.Client;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Observing_notifications
    {
        static readonly IActorSystem system = ActorSystem.Instance;

        [Test]
        public async void Client_to_actor()
        {
            var actor = system.FreshActorOf<TestActor>();

            using (var observer = await Observer.Create())
            {
                await actor.Tell(new Attach(observer));

                ActorRef sender = null;
                TextChanged @event = null;

                var done = new AutoResetEvent(false);
                var subscription = observer.Subscribe(notification =>
                {
                    sender = notification.Sender;
                    @event = (TextChanged) notification.Message;
                    done.Set();
                });

                await actor.Tell(new SetText("c-a"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(sender, Is.EqualTo(actor));
                Assert.That(@event.Text, Is.EqualTo("c-a"));

                subscription.Dispose();

                await actor.Tell(new SetText("kaboom"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(@event.Text, Is.EqualTo("c-a"));
            }
        }
    }
}