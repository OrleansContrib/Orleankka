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
            protected readonly List<string> received = new List<string>();

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
            readonly string provider;
            readonly TimeSpan timeout;
            readonly IActorSystem system;

            public TestCases(string provider, TimeSpan timeout)
            {
                this.provider = provider;
                this.timeout = timeout;
                system = TestActorSystem.Instance;
            }

            public async Task Client_to_stream<T>() where T : Actor
            {
                var stream = system.StreamOf(provider, "cs");

                await stream.Push("ce");
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"ce"}));
            }

            public async Task Actor_to_stream<T>() where T : Actor
            {
                var stream = system.StreamOf(provider, "as");
                var producer = system.ActorOf<TestProducerActor>("foo");

                await producer.Tell(new Produce {Stream = stream, Item = "ae"});
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received, Is.EquivalentTo(new[] {"ae"}));
            }

            public async Task Multistream_subscription_with_fixed_ids<T>() where T : Actor
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

            public async Task Multistream_subscription_based_on_regex_matching<T>() where T : Actor
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

            public async Task Declared_handler_only_automatic_item_filtering<T>() where T : Actor
            {
                var stream = system.StreamOf(provider, "declared-auto");
                Assert.DoesNotThrow(async ()=> await stream.Push(123),
                    "Should not throw handler not found exception");

                await stream.Push("e-123");
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-123"));
            }

            public async Task Select_all_filter<T>() where T : Actor
            {
                var stream = system.StreamOf(provider, "select-all");

                await stream.Push(42);
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("42"));
            }

            public async Task Explicit_filter<T>() where T : Actor
            {
                var stream = system.StreamOf(provider, "filtered");

                await stream.Push("f-001");
                await stream.Push("f-002");
                await Task.Delay(timeout);

                var consumer = system.ActorOf<T>("#");
                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(0));
            }

            public async Task Dynamic_target_selection<T>() where T : Actor
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

            [StreamSubscription(Source = "sms:declared-auto", Target = "#")]
            class TestDeclaredHandlerOnlyAutomaticFilterActor : TestConsumerActorBase
            {}

            [StreamSubscription(Source = "sms:select-all", Target = "#", Filter = "*")]
            class TestSelectAllFilterActor : TestConsumerActorBase
            {
                public override Task<object> OnReceive(object message)
                {
                    if (message is int)
                    {
                        received.Add(message.ToString());
                        return Task.FromResult<object>(null);
                    }

                    return base.OnReceive(message);
                }
            }

            [StreamSubscription(Source = "sms:filtered", Target = "#", Filter = "SelectItem()")]
            class TestExplicitFilterActor : TestConsumerActorBase
            {
                public static bool SelectItem(object item) => false;
            }

            [StreamSubscription(Source = "sms:dynamic-target", Target = "ComputeTarget()")]
            class TestDynamicTargetSelectorActor : TestConsumerActorBase
            {
                public static string ComputeTarget(object item) => $"{item}-pill";
            }

            [TestFixture, RequiresSilo]
            public class Tests
            {
                static TestCases Verify() =>
                   new TestCases("sms", TimeSpan.FromMilliseconds(100));

                [Test] public async Task Client_to_stream()                                 => await Verify().Client_to_stream<TestClientToStreamConsumerActor>();
                [Test] public async Task Actor_to_stream()                                  => await Verify().Actor_to_stream<TestActorToStreamConsumerActor>();
                [Test] public async Task Multistream_subscription_with_fixed_ids()          => await Verify().Multistream_subscription_with_fixed_ids<TestMultistreamSubscriptionWithFixedIdsActor>();
                [Test] public async Task Multistream_subscription_based_on_regex_matching() => await Verify().Multistream_subscription_based_on_regex_matching<TestMultistreamRegexBasedSubscriptionActor>();
                [Test] public async Task Declared_handler_only_automatic_item_filtering()   => await Verify().Declared_handler_only_automatic_item_filtering<TestDeclaredHandlerOnlyAutomaticFilterActor>();
                [Test] public async Task Select_all_filter()                                => await Verify().Select_all_filter<TestSelectAllFilterActor>();
                [Test] public async Task Explicit_filter()                                  => await Verify().Explicit_filter<TestExplicitFilterActor>();
                [Test] public async Task Dynamic_target_selection()                         => await Verify().Dynamic_target_selection<TestDynamicTargetSelectorActor>();
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

            [StreamSubscription(Source = "aqp:declared-auto", Target = "#")]
            class TestDeclaredHandlerOnlyAutomaticFilterActor : TestConsumerActorBase
            {}

            [StreamSubscription(Source = "aqp:select-all", Target = "#", Filter = "*")]
            class TestSelectAllFilterActor : TestConsumerActorBase
            {
                public override Task<object> OnReceive(object message)
                {
                    if (message is int)
                    {
                        received.Add(message.ToString());
                        return Task.FromResult<object>(null);
                    }

                    return base.OnReceive(message);
                }
            }

            [StreamSubscription(Source = "aqp:filtered", Target = "#", Filter = "SelectItem()")]
            class TestExplicitFilterActor : TestConsumerActorBase
            {
                public static bool SelectItem(object item) => false;
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
               static TestCases Verify() =>
                  new TestCases("aqp", TimeSpan.FromSeconds(5));

                [Test] public async Task Client_to_stream()                                 => await Verify().Client_to_stream<TestClientToStreamConsumerActor>();
                [Test] public async Task Actor_to_stream()                                  => await Verify().Actor_to_stream<TestActorToStreamConsumerActor>();
                [Test] public async Task Multistream_subscription_with_fixed_ids()          => await Verify().Multistream_subscription_with_fixed_ids<TestMultistreamSubscriptionWithFixedIdsActor>();
                [Test] public async Task Multistream_subscription_based_on_regex_matching() => await Verify().Multistream_subscription_based_on_regex_matching<TestMultistreamRegexBasedSubscriptionActor>();
                [Test] public async Task Declared_handler_only_automatic_item_filtering()   => await Verify().Declared_handler_only_automatic_item_filtering<TestDeclaredHandlerOnlyAutomaticFilterActor>();
                [Test] public async Task Select_all_filter()                                => await Verify().Select_all_filter<TestSelectAllFilterActor>();
                [Test] public async Task Explicit_filter()                                  => await Verify().Explicit_filter<TestExplicitFilterActor>();
                [Test] public async Task Dynamic_target_selection()                         => await Verify().Dynamic_target_selection<TestDynamicTargetSelectorActor>();
             }
        }
    }
}