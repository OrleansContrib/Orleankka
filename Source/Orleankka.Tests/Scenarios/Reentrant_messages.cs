using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    [TestFixture]
    public class Reentrant_messages
    {
        static readonly IActorSystem system = ActorSystem.Instance;

        [Test]
        public async void Should_deliver_to_interleaved_receive_channel()
        {
            var reentrant = system.FreshActorOf<TestReentrantActor>();
            Assert.That(await reentrant.Ask<bool>(new RegularMessage()), Is.False);
            Assert.That(await reentrant.Ask<bool>(new ReentrantMessage()), Is.True);
        }

        class RegularMessage
        {}

        class ReentrantMessage
        {}

        [Reentrant(typeof(ReentrantMessage))]
        class TestReentrantActor : Actor
        {
            public bool Handle(RegularMessage message)
            {
                return CallContext.LogicalGetData("ReceiveReentrant") == message;
            }

            public bool Handle(ReentrantMessage message)
            {
                return CallContext.LogicalGetData("ReceiveReentrant") == message;
            }
        }
    }
}
