using System;
using System.Linq;
using System.Threading;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Observing_actors
    {
        IActorSystem system;
        IActorRef actor;
        IClientObservable observer;

        [SetUp]
        public void SetUp()
        {
            system = new ActorSystem();
            actor = system.ActorOf<ITestActor>("test");
            
            observer = ClientObservable.Create().Result;
            actor.Tell(new Attach(observer)).Wait();
        }

        [TearDown]
        public void TearDown()
        {
            observer.Dispose();
        }

        [Test]
        public async void Should_notify_via_callback()
        {
            ActorPath source = null;
            FooPublished @event = null;

            var received = new AutoResetEvent(false);
            observer.Subscribe(e =>
            {
                source = e.Source;
                @event = e.Message as FooPublished;
                received.Set();
            });
            
            await actor.Tell(new PublishFoo {Foo = "foo"});
            received.WaitOne(TimeSpan.FromSeconds(5));

            Assert.AreEqual(new ActorPath(typeof(ITestActor), "test"), source);
            Assert.AreEqual("foo", @event.Foo);
        }
    }
}