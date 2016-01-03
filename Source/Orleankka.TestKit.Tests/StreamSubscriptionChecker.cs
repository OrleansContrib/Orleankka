using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    [StreamSubscription(Source = "sms:/TEST-(?<id>.*)/", Target = "{id}-test")]
    [StreamSubscription(Source = "sms:/SECOND-(.*)/", Target = "second")]
    public class Subscriber : Actor
    {
        public void Handler(int t)
        {

        }
    }


    [TestFixture]
    public class StreamSubscriptionChecker
    {
        [Test]
        [TestCase("sms:TEST-123", "123-test", true)]
        [TestCase("sms:TEST-123", "222-test", false)]
        [TestCase("sms:SECOND-123", "second", true)]
        [TestCase("sms:SECOND-123", "other", false)]
        public async Task Returns_true_if_target_id_matches_specification(string stream, string target, bool result)
        {
            var recieves = await
                           StreamSubscriptionValidator<Subscriber>.SubscribesToMessagesFrom
                                (stream, target);
            Assert.That(recieves, Is.EqualTo(result));
        }


    }
}
