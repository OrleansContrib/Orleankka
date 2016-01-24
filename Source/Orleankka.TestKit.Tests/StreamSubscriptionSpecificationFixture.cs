using NUnit.Framework;

namespace Orleankka.TestKit
{
    [StreamSubscription(Source = "sms:/TEST-(?<id>.*)/", Target = "{id}-test")]
    [StreamSubscription(Source = "sms:/SECOND-(.*)/", Target = "second")]
    public class Subscriber : Actor
    {}

    [TestFixture]
    public class StreamSubscriptionSpecificationFixture
    {
        [Test]
        [TestCase("sms:TEST-123", "123-test", true)]
        [TestCase("sms:TEST-123", "222-test", false)]
        [TestCase("sms:SECOND-123", "second", true)]
        [TestCase("sms:SECOND-123", "other",  false)]
        public void Checking_target_matches_specification(string stream, string target, bool result)
        {
            Assert.That(StreamSubscriptionSpecification<Subscriber>.Matches(stream, target), 
                Is.EqualTo(result));
        }
    }
}
