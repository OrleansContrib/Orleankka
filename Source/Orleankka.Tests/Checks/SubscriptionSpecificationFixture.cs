using NUnit.Framework;

namespace Orleankka.Checks
{
    using Core;

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
            var attribute = new StreamSubscriptionAttribute
            {
                Source = source,
                Target = target
            };

            var specification = StreamSubscriptionSpecification.From(typeof(Actor), attribute);
            var match = specification.Match(streamId);

            Assert.That(match.ActorId, Is.EqualTo(actorId));
        }

        [Test]
        [TestCase("sms:/INV-(?<id>[0-9]+)/", "S-{id}", "INV-001", "S-001")]
        [TestCase("sms:/(?<acc>[0-9]+)-(?<topic>[0-9]+)/", "{acc}-topics", "111-200", "111-topics")]
        public void Regex_based_matching(string source, string target, string streamId, string actorId)
        {
            var attribute = new StreamSubscriptionAttribute
            {
                Source = source,
                Target = target
            };

            var specification = StreamSubscriptionSpecification.From(typeof(Actor), attribute);
            var match = specification.Match(streamId);

            Assert.That(match.ActorId, Is.EqualTo(actorId));
        }
    }
}
