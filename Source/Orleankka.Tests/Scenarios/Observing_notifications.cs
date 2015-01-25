using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Observing_notifications
    {
        static readonly IActorSystem system = new ActorSystem();

        [Test]
        public async void Client_to_actor()
        {
            var actor = system.FreshActorOf<ITestActor>();

            using (var observable = await ClientObservable.Create())
            {
                await actor.Tell(new Attach(observable));

                Notification notification = null;
                var done = new AutoResetEvent(false);

                var subscription = observable.Subscribe(x =>
                {
                    notification = x;
                    done.Set();
                });
                
                await actor.Tell(new SetText("c-a"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(notification.Source, Is.EqualTo(actor.Path));
                Assert.That(notification.Message, Is.EqualTo("c-a"));

                subscription.Dispose();

                await actor.Tell(new SetText("kaboom"));
                done.WaitOne(TimeSpan.FromSeconds(5));

                Assert.That(notification.Message, Is.EqualTo("c-a"));
            }            
        }
        
        [Test]
        public async void Actor_to_actor()
        {
            var one = system.FreshActorOf<ITestInsideActor>();
            var another = system.FreshActorOf<ITestActor>();

            await one.Tell(new DoAttach(another.Path));
            await another.Tell(new SetText("a-a"));

            await Task.Delay(TimeSpan.FromSeconds(2));

            var received = await one.Ask<Notification[]>(new GetReceivedNotifications());
            Assert.That(received.Length, Is.EqualTo(1));

            var notification = received[0];
            Assert.That(notification.Source, Is.EqualTo(another.Path));
            Assert.That(notification.Message, Is.EqualTo("a-a"));
        }
    }
}