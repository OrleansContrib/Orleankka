using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Reentrant_messages
    {
        using Meta;
        using Testing;
        using Annotations;

        [Serializable]
        class RegularMessage : Query<bool>
        {}

        [Serializable]
        class ReentrantMessage : Query<bool>
        {}

        abstract class TestActor : Actor
        {
            protected internal override void Define()
            {
                On((RegularMessage x) => ReceivedReentrant(x));
                On((ReentrantMessage x) => ReceivedReentrant(x));
            }

            static bool ReceivedReentrant(object message)
            {
                return CallContext.LogicalGetData("LastMessageReceivedReentrant") == message;
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
                var actor = system.FreshActorOf<TestReentrantDefinedViaAttributeActor>();
                Assert.That(await actor.Ask(new RegularMessage()), Is.False);
                Assert.That(await actor.Ask(new ReentrantMessage()), Is.True);
            }

            [Test]
            public async void Could_be_defined_via_prototype()
            {
                var actor = system.FreshActorOf<TestReentrantDefinedViaPrototypeActor>();
                Assert.That(await actor.Ask(new RegularMessage()), Is.False);
                Assert.That(await actor.Ask(new ReentrantMessage()), Is.True);
            }

            [Test]
            public void Cannot_be_defined_outside_of_prototype_definition()
            {
                var actor = system.FreshActorOf<TestReentrantDefinedOutsideOfPrototypeActor>();
                Assert.Throws<InvalidOperationException>(async () => await actor.Tell("boo"));
            }

            [Reentrant(typeof(ReentrantMessage))]
            [UsedImplicitly]
            class TestReentrantDefinedViaAttributeActor : TestActor
            {}

            [UsedImplicitly]
            class TestReentrantDefinedViaPrototypeActor : TestActor
            {
                protected internal override void Define()
                {
                    Reentrant(x => x is ReentrantMessage);
                    base.Define();
                }
            }

            [UsedImplicitly]
            class TestReentrantDefinedOutsideOfPrototypeActor : TestActor
            {
                public void Handle(string message)
                {
                    Reentrant(x => x is ReentrantMessage);
                }
            }
        }
    }
}