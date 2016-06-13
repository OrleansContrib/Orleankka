using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;

using NUnit.Framework;

namespace Orleankka.Features
{
    using CSharp;
	
    namespace Reentrant_messages
    {
        using Meta;
        using Testing;

        [Serializable]
        class RegularMessage : Query<bool>
        {}

        [Serializable]
        class ReentrantMessage : Query<bool>
        {}

        [Reentrant(typeof(ReentrantMessage))]
        class TestActor : Actor
        {
            bool On(RegularMessage x)   => ReceivedReentrant(x);
            bool On(ReentrantMessage x) => ReceivedReentrant(x);

            static bool ReceivedReentrant(object message) => 
                CallContext.LogicalGetData("LastMessageReceivedReentrant") == message;
        }
                
        class TestActor2 : Actor
        {
            bool On(RegularMessage x) => ReceivedReentrant(x);
            bool On(ReentrantMessage x) => ReceivedReentrant(x);

            static bool ReceivedReentrant(object message) =>
                CallContext.LogicalGetData("LastMessageReceivedReentrant") == message;

            static bool IsReentrant(object message)
            {
                if (message is ReentrantMessage) return true;

                return false;
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
            public async void Could_be_defined_via_attribute()
            {
                var actor = system.FreshActorOf<TestActor>();
                Assert.That(await actor.Ask(new RegularMessage()), Is.False);
                Assert.That(await actor.Ask(new ReentrantMessage()), Is.True);
            }
        }
    }
}