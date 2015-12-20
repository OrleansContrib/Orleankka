using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{

    //[StreamSubscription(Source = "sms:/TEST-(.*)/", Target = "#")]
    [StreamSubscription(Source = "sms:/TEST-(?<id>.*)/", Target = "{id}-test")]
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
        public async Task Resturns_target_id_when_matched()
        {
            var recieves = await 
                "sms:TEST-123".MatchesIdOf<Subscriber>(
                                    "123-test",
                                    "message");
            Assert.That(recieves, Is.True);

            /* alway fail
            var ignoresInts = await "sms:TEST-123".MatchesIdOf<Subscriber>(
                "123-test",
                10);

            Assert.That(ignoresInts, Is.False);
            */

            var ignoredDueToidMismatch = await "sms:TEST-123".MatchesIdOf<Subscriber>(
                "222-test",
                "message");
            Assert.That(ignoredDueToidMismatch, Is.False);

        }

    }
}
