using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleankka.Core;

using Orleans;

namespace Orleankka.Checks
{
    using Core.Streams;

    [TestFixture]
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

            var specification = StreamSubscriptionSpecification.From(typeof(Actor), attribute);
            var match = specification.Match(system, streamId);

            if (match == StreamSubscriptionMatch.None)
            {
                Assert.That(actorId, Is.Null);
                return;
            }

            var message = new object();
            match.Receiver(message);

            Assert.That(system.RequestedRef, Is.Not.Null);
            Assert.That(system.RequestedRef.Path, Is.EqualTo(ActorPath.From(typeof(Actor), actorId)));
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

            var specification = StreamSubscriptionSpecification.From(typeof(Actor), attribute);
            var match = specification.Match(system, streamId);

            if (match == StreamSubscriptionMatch.None)
            {
                Assert.That(actorId, Is.Null);
                return;
            }

            var message = new object();
            match.Receiver(message);

            Assert.That(system.RequestedRef, Is.Not.Null);
            Assert.That(system.RequestedRef.Path, Is.EqualTo(ActorPath.From(typeof(Actor), actorId)));
            Assert.That(system.RequestedRef.MessagePassedToTell, Is.SameAs(message));
        }

        [Test]
        public void Dynamic_target_matching()
        {
            var system = new ActorSystemMock();

            var type = ActorType.From(typeof(DynamicTargetSelectorActor));
            var specification = StreamSubscriptionSpecification.From(type).ElementAt(0);
            var match = specification.Match(system, "foo");

            var message = new object();
            match.Receiver(message);

            Assert.That(system.RequestedRef, Is.Not.Null);
            Assert.That(system.RequestedRef.Path, Is.EqualTo(ActorPath.From(typeof(DynamicTargetSelectorActor), "bar")));
            Assert.That(system.RequestedRef.MessagePassedToTell, Is.SameAs(message));
        }

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

            public ActorRef ActorOf(Type type, string id)
            {
                return ActorOf(ActorPath.From(type, id));
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
