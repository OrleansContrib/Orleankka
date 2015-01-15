using System;
using System.Linq;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Request_response
    {
        IActorSystem system;
        IActorRef actor;

        [SetUp]
        public void SetUp()
        {
            system = new ActorSystem();
            actor  = system.ActorOf<ITestActor>("test");
        }

        [Test]
        public void When_tell()
        {
            Assert.DoesNotThrow(
                async ()=> await actor.Tell(new DoFoo()));
        }

        [Test]
        public async void When_ask()
        {
            await actor.Tell(new DoFoo {Text = "foo"});

            Assert.AreEqual("foo-test", 
                await actor.Ask(new GetFoo()));
        }
    }
}