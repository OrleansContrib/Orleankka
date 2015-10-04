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
        class Received : Query<List<string>>
        {}

        [Serializable]
        class Deactivate : Command
        {}

        abstract class TestConsumerActorBase : Actor
        {
            readonly List<string> received = new List<string>();

            void On(string x) => received.Add(x);
            List<string> On(Received x) => received;

            void On(Deactivate x) => Activation.DeactivateOnIdle();
        }

        [Serializable]
        class Produce : Command
        {
            public StreamRef Stream;
            public string Item;
        }

        class TestProducerActor : Actor
        {
            Task On(Produce x) => x.Stream.Push(x.Item);
        }

        [StreamSubscription(Source = "sms:cs", Target = "#")]
        class TestClientToStreamConsumerActor : TestConsumerActorBase
        {}

        [StreamSubscription(Source = "sms:as", Target = "#")]
        class TestActorToStreamConsumerActor : TestConsumerActorBase
        {}

        [StreamSubscription(Source = "sms:a", Target = "#")]
        [StreamSubscription(Source = "sms:b", Target = "#")]
        class TestMultistreamSubscriptionWithFixedIdsActor : TestConsumerActorBase
        {}

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
            public async void Client_to_stream()
            {
                var stream = system.StreamOf("sms", "cs");

                await stream.Push("e");
                await Task.Delay(100);

                var consumer = system.ActorOf<TestClientToStreamConsumerActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"e"}));
            }

            [Test]
            public async void Actor_to_stream()
            {
                var stream = system.StreamOf("sms", "as");
                var producer = system.ActorOf<TestProducerActor>("foo");

                await producer.Tell(new Produce {Stream = stream, Item = "e"});
                await Task.Delay(100);

                var consumer = system.ActorOf<TestActorToStreamConsumerActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"e"}));
            }

            [Test]
            public async void Multistream_subscription_with_fixed_ids()
            {
                var a = system.StreamOf("sms", "a");
                var b = system.StreamOf("sms", "b");

                await a.Push("a-123");
                await b.Push("b-456");
                await Task.Delay(100);

                var consumer = system.ActorOf<TestMultistreamSubscriptionWithFixedIdsActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"a-123", "b-456"}));
            }
        }
    }
}