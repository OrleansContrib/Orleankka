using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;
using Orleans;

namespace Orleankka.Checks
{
    using Testing;
    using Core.Streams;

    [TestFixture, RequiresSilo]
    public class SubscriptionSpecificationFixture
    {
        [Test]
        [TestCase("sms:a", "#", "as", null)]
        [TestCase("sms:a", "#", "a",  "#")]
        [TestCase("sms:a", "{x}", "a", "{x}")]
        [TestCase("sms:a(.+)", "#", "a-", null)]
        [TestCase("sms:a(.+)", "#", "a(.+)", "#")]
        public void Matching_by_fixed_ids(string source, string target, string streamId, string actorId)
        {
            var system = new ActorSystemMock();

            var attribute = new StreamSubscriptionAttribute
            {
                Source = source,
                Target = target
            };

            var type = typeof(TestActor);
            var dispatcher = new Dispatcher(type);

            var specification = StreamSubscriptionBinding.From(type, attribute, dispatcher);
            specification.Type = ActorTypeName.Of(typeof(TestActor));

            var match = specification.Match(system, streamId);
            if (match == StreamSubscriptionMatch.None)
            {
                Assert.That(actorId, Is.Null);
                return;
            }

            var message = new object();
            match.Receiver(message);

            Assert.That(system.RequestedRef, Is.Not.Null);
            Assert.That(system.RequestedRef.Path, Is.EqualTo(typeof(TestActor).ToActorPath(actorId)));
            Assert.That(system.RequestedRef.MessagePassedToTell, Is.SameAs(message));
        }

        [Test]
        [TestCase("sms:/INV-(?<id>[0-9]+)/", "S-{id}", "INV-001", "S-001")]
        [TestCase("sms:/(?<acc>[0-9]+)-(?<topic>[0-9]+)/", "{acc}-topics", "111-200", "111-topics")]
        public void Regex_based_matching(string source, string target, string streamId, string actorId)
        {
            var system = new ActorSystemMock();

            var attribute = new StreamSubscriptionAttribute
            {
                Source = source,
                Target = target
            };

            var type = typeof(TestActor);
            var dispatcher = new Dispatcher(type);

            var specification = StreamSubscriptionBinding.From(type, attribute, dispatcher);
            specification.Type = ActorTypeName.Of(typeof(TestActor));

            var match = specification.Match(system, streamId);
            if (match == StreamSubscriptionMatch.None)
            {
                Assert.That(actorId, Is.Null);
                return;
            }

            var message = new object();
            match.Receiver(message);

            Assert.That(system.RequestedRef, Is.Not.Null);
            Assert.That(system.RequestedRef.Path, Is.EqualTo(typeof(TestActor).ToActorPath(actorId)));
            Assert.That(system.RequestedRef.MessagePassedToTell, Is.SameAs(message));
        }

        [Test]
        public void Dynamic_target_matching()
        {
            var system = new ActorSystemMock();

            var type = typeof(DynamicTargetSelectorActor);
            var dispatcher = new Dispatcher(type);

            var specification = StreamSubscriptionBinding.From(type, dispatcher).ElementAt(0);
            specification.Type = ActorTypeName.Of(typeof(DynamicTargetSelectorActor));

            var match = specification.Match(system, "foo");

            var message = new object();
            match.Receiver(message);

            Assert.That(system.RequestedRef, Is.Not.Null);
            Assert.That(system.RequestedRef.Path, Is.EqualTo(typeof(DynamicTargetSelectorActor).ToActorPath("bar")));
            Assert.That(system.RequestedRef.MessagePassedToTell, Is.SameAs(message));
        }

        class TestActor : Actor {}

        [StreamSubscription(Source = "sms:foo", Target = "ComputeSubscriptionTarget()")]
        class DynamicTargetSelectorActor : Actor
        {
            public static string ComputeSubscriptionTarget(object item) => "bar";
        }

        class ActorSystemMock : IActorSystem
        {
            public ActorRefMock RequestedRef;

            public ActorRef ActorOf(ActorPath path)
            {
                return (RequestedRef = new ActorRefMock(path));
            }

            public StreamRef StreamOf(StreamPath path)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        class ActorRefMock : ActorRef
        {
            public object MessagePassedToTell;

            public ActorRefMock(ActorPath path)
                : base(path)
            {}

            public override Task Tell(object message)
            {
                MessagePassedToTell = message;
                return TaskDone.Done;;
            }
        }
    }
}
