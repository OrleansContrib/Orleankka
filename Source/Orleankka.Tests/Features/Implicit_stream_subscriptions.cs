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

        abstract class Tests<TClientToStreamConsumerActor, TActorToStreamConsumerActor, TMultistreamSubscriptionWithFixedIdsActor, TMultistreamRegexBasedSubscriptionActor, TFilteredSubscriptionActor>
            where TClientToStreamConsumerActor : IActor
            where TActorToStreamConsumerActor : IActor
            where TMultistreamSubscriptionWithFixedIdsActor : IActor
            where TMultistreamRegexBasedSubscriptionActor : IActor
            where TFilteredSubscriptionActor : IActor
        {
            IActorSystem system;

            [SetUp]
            public void SetUp() => system = TestActorSystem.Instance;

            [Test]
            public async void Client_to_stream()
            {
                var stream = system.StreamOf(Provider, "cs");

                await stream.Push("ce");
                await Task.Delay(Timeout);

                var consumer = system.ActorOf<TClientToStreamConsumerActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"ce"}));
            }

            [Test]
            public async void Actor_to_stream()
            {
                var stream = system.StreamOf(Provider, "as");
                var producer = system.ActorOf<TestProducerActor>("foo");

                await producer.Tell(new Produce {Stream = stream, Item = "ae"});
                await Task.Delay(Timeout);

                var consumer = system.ActorOf<TActorToStreamConsumerActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"ae"}));
            }

            [Test]
            public async void Multistream_subscription_with_fixed_ids()
            {
                var a = system.StreamOf(Provider, "a");
                var b = system.StreamOf(Provider, "b");

                await a.Push("a-001");
                await b.Push("b-001");
                await Task.Delay(Timeout);

                var consumer = system.ActorOf<TMultistreamSubscriptionWithFixedIdsActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"a-001", "b-001"}));
            }

            [Test]
            public async void Multistream_subscription_based_on_regex_matching()
            {
                var s1 = system.StreamOf(Provider, "INV-001");
                var s2 = system.StreamOf(Provider, "INV-002");

                await s1.Push("001");
                await s2.Push("002");
                await Task.Delay(Timeout);

                var consumer = system.ActorOf<TMultistreamRegexBasedSubscriptionActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"001", "002"}));
            }

            [Test]
            public async void Filtering_items()
            {
                var stream = system.StreamOf(Provider, "filtered");

                await stream.Push("f-001");
                await stream.Push("f-002");
                await Task.Delay(Timeout);

                var consumer = system.ActorOf<TFilteredSubscriptionActor>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(0));
            }

            protected abstract string Provider  { get; }
            protected abstract TimeSpan Timeout { get; }
        }

        namespace SimpleMessageStreamProviderVerification
        {
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

            [StreamSubscription(Source = "sms:/INV-([0-9]+)/", Target = "#")]
            class TestMultistreamRegexBasedSubscriptionActor : TestConsumerActorBase
            {}

            [StreamSubscription(Source = "sms:filtered", Target = "#", Filter = "Select")]
            class TestFilteredSubscriptionActor : TestConsumerActorBase
            {
                public static bool Select(object item) => false;
            }

            [TestFixture, RequiresSilo(Fresh = true)]
            class Tests : Tests<
                TestClientToStreamConsumerActor, 
                TestActorToStreamConsumerActor, 
                TestMultistreamSubscriptionWithFixedIdsActor, 
                TestMultistreamRegexBasedSubscriptionActor, 
                TestFilteredSubscriptionActor>
            {
                protected override string Provider  => "sms";
                protected override TimeSpan Timeout => TimeSpan.FromMilliseconds(100);
            }
        }

        namespace AzureQueueStreamProviderVerification
        {
            [StreamSubscription(Source = "aqp:cs", Target = "#")]
            class TestClientToStreamConsumerActor : TestConsumerActorBase
            {}

            [StreamSubscription(Source = "aqp:as", Target = "#")]
            class TestActorToStreamConsumerActor : TestConsumerActorBase
            {}

            [StreamSubscription(Source = "aqp:a", Target = "#")]
            [StreamSubscription(Source = "aqp:b", Target = "#")]
            class TestMultistreamSubscriptionWithFixedIdsActor : TestConsumerActorBase
            {}

            [StreamSubscription(Source = "aqp:/INV-([0-9]+)/", Target = "#")]
            class TestMultistreamRegexBasedSubscriptionActor : TestConsumerActorBase
            {}

            [StreamSubscription(Source = "aqp:filtered", Target = "#", Filter = "Select")]
            class TestFilteredSubscriptionActor : TestConsumerActorBase
            {
                public static bool Select(object item) => false;
            }

            [TestFixture, RequiresSilo(Fresh = true), Category("Slow"), Explicit]
            class Tests : Tests<
               TestClientToStreamConsumerActor,
               TestActorToStreamConsumerActor,
               TestMultistreamSubscriptionWithFixedIdsActor,
               TestMultistreamRegexBasedSubscriptionActor, 
               TestFilteredSubscriptionActor>
            {
                protected override string Provider  => "aqp";
                protected override TimeSpan Timeout => TimeSpan.FromSeconds(5);
            }
        }
    }
}