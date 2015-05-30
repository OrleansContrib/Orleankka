using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features.Typed_actors
{
    namespace Reentrant_invocations
    {
        using Typed;
        using Testing;

        class TestActor : TypedActor
        {
            object invocation;

            public bool NonReentrant()
            {
                return ReceivedReentrant(invocation);
            }

            public virtual bool Reentrant()
            {
                return ReceivedReentrant(invocation);
            }

            protected internal override Task<object> OnReceive(object message)
            {
                invocation = message;
                return base.OnReceive(message);
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
                var actor = system.FreshTypedActorOf<TestReentrantDefinedViaAttributeActor>();

                Assert.IsFalse(await actor.Call(x => x.NonReentrant()));
                Assert.IsTrue(await actor.Call(x => x.Reentrant()));
            }            
            
            [Test]
            public async void Could_be_defined_via_prototype()
            {
                var actor = system.FreshTypedActorOf<TestReentrantDefinedViaPrototypeActor>();

                Assert.IsFalse(await actor.Call(x => x.NonReentrant()));
                Assert.IsTrue(await actor.Call(x => x.Reentrant()));
            }
        }

        class TestReentrantDefinedViaAttributeActor : TestActor
        {
            [Reentrant]
            public override bool Reentrant()
            {
                return base.Reentrant();
            }
        }

        class TestReentrantDefinedViaPrototypeActor : TestActor
        {
            protected internal override void Define()
            {
                Reentrant(x => ((Invocation)x).Member == "Reentrant");
            }
        }
    }
}