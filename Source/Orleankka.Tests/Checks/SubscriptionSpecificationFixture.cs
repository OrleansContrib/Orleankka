using System;

using NUnit.Framework;

namespace Orleankka.Checks
{
    using Core;

    [TestFixture, Ignore("TODO")]
    public class SubscriptionSpecificationFixture
    {
        static StreamSubscriptionSpecification Specification(string source, string target = null) => new StreamSubscriptionSpecification(source, target, null);

        [Test]
        public void Do_not_allow_duplicate_placeholders()
        {
            Assert.Throws<InvalidOperationException>(() => Specification("{id}-{id}"));
        }

        [Test]
        public void Placeholder_cannot_be_immediately_followed_by_another_placeholder()
        {
            Assert.Throws<InvalidOperationException>(() => Specification("{id}{id}"));
        }

        [Test]
        public void Any_chars_allowed_for_a_placeholder_name()
        {
            Assert.DoesNotThrow(() => Specification("INV-{{[\\^$.|?*+()}"));
            Assert.True(false); // match
        }

        [Test]
        public void Matching_stream_ids()
        {
            // TODO: ....
        }

        [Test]
        public void Generating_actor_ids()
        {
            // TODO: ....
        }
    }
}
