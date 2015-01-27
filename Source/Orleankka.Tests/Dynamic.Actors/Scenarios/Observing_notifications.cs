using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Dynamic.Actors.Scenarios
{
    [TestFixture]
    public class Observing_notifications
    {
        static readonly IActorSystem system = ActorSystem.Instance;

        [Test]
        public async void Client_to_actor()
        {
            var actor = system.FreshActorOf<TestActor>();

            using (var observable = await ClientObservable.Create())
            {
                await actor.Tell(new Attach(observable));

                TextChanged @event = null;
                ActorPath source = null;

                var done = new AutoResetEvent(false);
                var subscription = observable.Subscribe(notification =>
                {
                    @event = (TextChanged) notification.Message;
                    source = notification.Source;
                    done.Set();
                });

                await actor.Tell(new SetText("c-a"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(source, Is.EqualTo(actor.Path));
                Assert.That(@event.Text, Is.EqualTo("c-a"));

                subscription.Dispose();

                await actor.Tell(new SetText("kaboom"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(@event.Text, Is.EqualTo("c-a"));
            }
        }
        
        [Test]
        public async void Actor_to_actor()
        {
            var one = system.FreshActorOf<TestInsideActor>();
            var another = system.FreshActorOf<TestActor>();

            await one.Tell(new DoAttach(another));
            await another.Tell(new SetText("a-a"));

            await Task.Delay(TimeSpan.FromSeconds(2));

            var received = await one.Ask<Notification[]>(new GetReceivedNotifications());
            Assert.That(received.Length, Is.EqualTo(1));

            var @event = (TextChanged)received[0].Message;
            var source = received[0].Source;

            Assert.That(source, Is.EqualTo(another));
            Assert.That(@event.Text, Is.EqualTo("a-a"));
        }
    }
}