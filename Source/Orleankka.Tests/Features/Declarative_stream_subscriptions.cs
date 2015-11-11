using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Declarative_stream_subscriptions
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

        class TestCases
        {
            IActorSystem system;

            readonly string provider;
            readonly TimeSpan timeout;

            public TestCases(string provider, TimeSpan timeout)
            {
                this.provider = provider;
                this.timeout = timeout;
            }

            public void SetUp() => system = TestActorSystem.Instance;

            public async Task Client_to_stream<T>() where T : IActor
            {
                var stream = system.StreamOf(provider, "cs");

                await stream.Push("ce");
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"ce"}));
            }

            public async Task Actor_to_stream<T>() where T : IActor
            {
                var stream = system.StreamOf(provider, "as");
                var producer = system.ActorOf<TestProducerActor>("foo");

                await producer.Tell(new Produce {Stream = stream, Item = "ae"});
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"ae"}));
            }

            public async Task Multistream_subscription_with_fixed_ids<T>() where T : IActor
            {
                var a = system.StreamOf(provider, "a");
                var b = system.StreamOf(provider, "b");

                await a.Push("a-001");
                await b.Push("b-001");
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"a-001", "b-001"}));
            }

            public async Task Multistream_subscription_based_on_regex_matching<T>() where T : IActor
            {
                var s1 = system.StreamOf(provider, "INV-001");
                var s2 = system.StreamOf(provider, "INV-002");

                await s1.Push("001");
                await s2.Push("002");
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"001", "002"}));
            }

            public async Task Filtering_items<T>() where T : IActor
            {
                var stream = system.StreamOf(provider, "filtered");

                await stream.Push("f-001");
                await stream.Push("f-002");
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(0));
            }

            public async Task Dynamic_target_selection<T>() where T : IActor
            {
                var stream = system.StreamOf(provider, "dynamic-target");

                await stream.Push("red");
                await stream.Push("blue");
                await Task.Delay(timeout);

                var consumer1 = system.ActorOf<T>("red-pill");
                var consumer2 = system.ActorOf<T>("blue-pill");

                Assert.That((await consumer1.Ask(new Received()))[0], Is.EqualTo("red"));
                Assert.That((await consumer2.Ask(new Received()))[0], Is.EqualTo("blue"));
            }
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

            [StreamSubscription(Source = "sms:filtered", Target = "#", Filter = "Select()")]
            class TestFilteredSubscriptionActor : TestConsumerActorBase
            {
                public static bool Select(object item) => false;
            }

            [StreamSubscription(Source = "sms:dynamic-target", Target = "ComputeTarget()")]
            class TestDynamicTargetSelectorActor : TestConsumerActorBase
            {
                public static string ComputeTarget(object item) => $"{item}-pill";
            }

            [TestFixture, RequiresSilo]
            public class Tests
            {
                TestCases verify;

                [SetUp]
                public void SetUp()
                {
                    verify = new TestCases(provider: "sms", timeout: TimeSpan.FromMilliseconds(100));
                    verify.SetUp();
                }

                [Test] public async Task Client_to_stream()                                 => await verify.Client_to_stream<TestClientToStreamConsumerActor>();
                [Test] public async Task Actor_to_stream()                                  => await verify.Actor_to_stream<TestActorToStreamConsumerActor>();
                [Test] public async Task Multistream_subscription_with_fixed_ids()          => await verify.Multistream_subscription_with_fixed_ids<TestMultistreamSubscriptionWithFixedIdsActor>();
                [Test] public async Task Multistream_subscription_based_on_regex_matching() => await verify.Multistream_subscription_based_on_regex_matching<TestMultistreamRegexBasedSubscriptionActor>();
                [Test] public async Task Filtering_items()                                  => await verify.Filtering_items<TestFilteredSubscriptionActor>();
                [Test] public async Task Dynamic_target_selection()                         => await verify.Dynamic_target_selection<TestDynamicTargetSelectorActor>();
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

            [StreamSubscription(Source = "aqp:filtered", Target = "#", Filter = "Select()")]
            class TestFilteredSubscriptionActor : TestConsumerActorBase
            {
                public static bool Select(object item) => false;
            }

            [StreamSubscription(Source = "aqp:dynamic-target", Target = "ComputeTarget()")]
            class TestDynamicTargetSelectorActor : TestConsumerActorBase
            {
                public static string ComputeTarget(object item) => $"{item}-pill";
            }

            [TestFixture]
            [RequiresSilo(Fresh = true, EnableAzureQueueStreamProvider = true)]
            [Category("Slow"), Explicit]
            public class Tests
            {
                TestCases verify;

                [SetUp]
                public void SetUp()
                {
                    verify = new TestCases(provider: "aqp", timeout: TimeSpan.FromSeconds(5));
                    verify.SetUp();
                }

                [Test] public async Task Client_to_stream()                                 => await verify.Client_to_stream<TestClientToStreamConsumerActor>();
                [Test] public async Task Actor_to_stream()                                  => await verify.Actor_to_stream<TestActorToStreamConsumerActor>();
                [Test] public async Task Multistream_subscription_with_fixed_ids()          => await verify.Multistream_subscription_with_fixed_ids<TestMultistreamSubscriptionWithFixedIdsActor>();
                [Test] public async Task Multistream_subscription_based_on_regex_matching() => await verify.Multistream_subscription_based_on_regex_matching<TestMultistreamRegexBasedSubscriptionActor>();
                [Test] public async Task Filtering_items()                                  => await verify.Filtering_items<TestFilteredSubscriptionActor>();
                [Test] public async Task Dynamic_target_selection()                         => await verify.Dynamic_target_selection<TestDynamicTargetSelectorActor>();
            }
        }
    }
}