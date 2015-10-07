using NUnit.Framework;

namespace Orleankka.Checks
{
    using Core;

    [TestFixture]
    public class SubscriptionSpecificationFixture
    {
        [Test]
        [TestCase("INV-001", "INV-(?<id>[0-9]+)", "S-{id}", "S-001")]
        [TestCase("111-200", "(?<account>[0-9]+)-(?<topic>[0-9]+)", "{account}-Topics", "111-Topics")]
        public void Matching_and_generating_actor_ids(string streamId, string source, string target, string actorId)
        {
            var attribute = new StreamSubscriptionAttribute
            {
                Source = $"sms:{source}",
                Target = target
            };

            var specification = StreamSubscriptionSpecification.From(typeof(Actor), attribute);
            var match = specification.Match(streamId);

            Assert.That(match.Id, Is.EqualTo(actorId));
        }
    }
}
