using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Implicit_stream_subscriptions
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Deactivate : Command
        {}

        [Serializable]
        public class Received : Query<List<string>>
        {}

        [StreamSubscription(Stream = "a", Actor = "#")]
        [StreamSubscription(Stream = "b", Actor = "#")]
        public class TestActor : Actor
        {
            readonly List<string> received = new List<string>();

            void On(string x) => received.Add(x);
            List<string> On(Received x) => received;

            void On(Deactivate x) => Activation.DeactivateOnIdle();
        }

        [TestFixture]
        [Explicit, Category("Slow")]
        [RequiresSilo(Fresh = true, DefaultKeepAliveTimeoutInMinutes = 1)]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async void Subscribing_to_multiple_streams_with_fixed_names()
            {
                var consumer = system.ActorOf<TestActor>("#");

                var a = system.StreamOf("sms", "a");
                var b = system.StreamOf("sms", "b");

                await a.Push("a-123");
                await b.Push("b-456");
                await Task.Delay(100);

                var received = await consumer.Ask(new Received());
                Assert.That(received,
                    Is.EquivalentTo(new[] {"a-123", "b-456"}));
            }
        }
    }
}