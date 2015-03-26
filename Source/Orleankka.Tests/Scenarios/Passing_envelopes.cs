using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Scenarios
{
    using Testing;

    [RequiresSilo]
    public class Passing_envelopes : ActorSystemScenario
    {
        [Test]
        public async void Unwrapping_and_dispatching_envelopes_is_done_by_application_code()
        {
            var actor = system.FreshActorOf<TestEnvelopeActor>();
            Assert.That(await actor.Ask<bool>(Wrap(new RegularMessage())), Is.False);
            Assert.That(await actor.Ask<bool>(Wrap(new ReentrantMessage())), Is.True);
        }

        static TestEnvelope Wrap(object message)
        {
            return new TestEnvelope {Body = message};
        }

        class TestEnvelope
        {
            public object Body;
        }

        class RegularMessage
        {}

        class ReentrantMessage
        {}

        class TestEnvelopeActor : Actor
        {
            TestEnvelope envelope;

            protected internal override void Define()
            {
                Reentrant(envelope => ((TestEnvelope)envelope).Body is ReentrantMessage);
            }

            public override Task<object> OnReceive(object message)
            {
                envelope = ((TestEnvelope)message);
                return DispatchAsync(envelope.Body);
            }

            public bool Handle(RegularMessage message)
            {
                return CallContext.LogicalGetData("ReceiveReentrant") == envelope;
            }

            public bool Handle(ReentrantMessage message)
            {
                return CallContext.LogicalGetData("ReceiveReentrant") == envelope;
            }
        }
    }
}
