using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Meta;
    using Testing;

    [RequiresSilo]
    public class Reentrant_messages : ActorSystemScenario
    {
        [Test]
        public async void Reentrant_could_be_defined_via_attribute()
        {
            var actor = system.FreshActorOf<TestReentrantDefinedViaAttributeActor>();
            Assert.That(await actor.Ask(new RegularMessage()), Is.False);
            Assert.That(await actor.Ask(new ReentrantMessage()), Is.True);
        }
        
        [Test]
        public async void Reentrant_could_be_defined_via_prototype()
        {
            var actor = system.FreshActorOf<TestReentrantDefinedViaPrototypeActor>();
            Assert.That(await actor.Ask(new RegularMessage()), Is.False);
            Assert.That(await actor.Ask(new ReentrantMessage()), Is.True);
        }

        [Test]
        public void Reentrant_cannot_be_defined_outside_of_prototype_definition()
        {
            var actor = system.FreshActorOf<TestReentrantDefinedOutsideOfPrototypeActor>();
            Assert.Throws<InvalidOperationException>(async ()=> await actor.Tell("boo"));
        }

        class RegularMessage : Query<bool>
        {}

        class ReentrantMessage : Query<bool>
        {}

        abstract class TestReentrantActorBase : Actor
        {
            protected internal override void Define()
            {
                On((RegularMessage x)   => ReceivedReentrant(x));
                On((ReentrantMessage x) => ReceivedReentrant(x));
            }

            static bool ReceivedReentrant(object message)
            {
                return CallContext.LogicalGetData("LastMessageReceivedReentrant") == message;
            }
        }

        [Reentrant(typeof(ReentrantMessage))]
        class TestReentrantDefinedViaAttributeActor : TestReentrantActorBase
        {}

        class TestReentrantDefinedViaPrototypeActor : TestReentrantActorBase
        {
            protected internal override void Define()
            {
                Reentrant(x => x is ReentrantMessage);
                
                base.Define();
            }
        }

        class TestReentrantDefinedOutsideOfPrototypeActor : TestReentrantActorBase
        {
            public void Handle(string message)
            {
                Reentrant(x => x is ReentrantMessage);
            }
        }
    }
}
