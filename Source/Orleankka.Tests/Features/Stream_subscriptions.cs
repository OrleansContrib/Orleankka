using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Stream_subscriptions
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Subscribe : Command
        {
            public StreamFilter Filter;
        }

        [Serializable]
        public class Kill : Command
        {}

        [Serializable]
        public class Received : Query<List<string>>
        {}
        
        public abstract class TestConsumerActorBase : ActorGrain
        {
            readonly List<string> received = new List<string>();

            Task On(Subscribe x) => Stream().Subscribe(this, x.Filter);
            void On(Kill x) => DeactivateOnIdle();

            void On(string x) => received.Add(x);
            List<string> On(Received x) => received;

            Task On(Activate _) => Stream().Resume(this);

            StreamRef Stream() => System.StreamOf(Provider, $"{Provider}-42");
            protected abstract string Provider { get; }

            protected override async Task<object> OnReceive(object message)
            {
                switch (message)
                {
                    case int x:
                        received.Add(x.ToString());
                        break;
                    default:
                        return await base.OnReceive(message);
                }

                return Done;
            }
        }

        class TestCases<TConsumer> where TConsumer : IActorGrain
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

            public async Task Resuming_on_reactivation()
            {
                var consumer = system.ActorOf<TConsumer>("cons");
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf(provider, $"{provider}-42");
                await stream.Push("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-123"));

                await consumer.Tell(new Kill());
                await Task.Delay(TimeSpan.FromSeconds(61));

                await stream.Push("e-456");
                await Task.Delay(timeout);

                received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-456"));
            }

            public async Task Subscription_is_idempotent()
            {
                var consumer = system.ActorOf<TConsumer>("idempotent");

                await consumer.Tell(new Subscribe());
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf(provider, $"{provider}-42");
                await stream.Push("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
            }

            public async Task Select_all_filter()
            {
                var consumer = system.ActorOf<TConsumer>("select-all");
                await consumer.Tell(new Subscribe {Filter = StreamFilter.ReceiveAll});

                var stream = system.StreamOf(provider, $"{provider}-42");
                await stream.Push(123);
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("123"));
            }

            public async Task Explicit_filter()
            {
                var consumer = system.ActorOf<TConsumer>("fff");

                var filter = new StreamFilter(DropAll);
                await consumer.Tell(new Subscribe { Filter = filter });

                var stream = system.StreamOf(provider, $"{provider}-filtered");
                await stream.Push("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(0));
            }

            static bool DropAll(object item) => false;
        }

        namespace SimpleMessageStreamProviderVerification
        {
            [TestFixture, RequiresSilo]
            class Tests
            {
                static TestCases<TestConsumerActor> Verify() => 
                   new TestCases<TestConsumerActor>("sms", TimeSpan.FromMilliseconds(100));

                [Test, Category("Slow")] public async Task Resuming_on_reactivation()       => await Verify().Resuming_on_reactivation();
                [Test] public async Task Subscription_is_idempotent()                       => await Verify().Subscription_is_idempotent();
                [Test] public async Task Select_all_filter()                                => await Verify().Select_all_filter();
                [Test] public async Task Explicit_filter()                                  => await Verify().Explicit_filter();
            }

            public interface ITestConsumerActor : IActorGrain
            {}

            public class TestConsumerActor : TestConsumerActorBase, ITestConsumerActor
            {
                protected override string Provider => "sms";
            }
        }

        namespace AzureQueueStreamProviderVerification
        {
            public interface ITestConsumerActor : IActorGrain
            {}

            public class TestConsumerActor : TestConsumerActorBase, ITestConsumerActor
            {
                protected override string Provider => "aqp";
            }

            [TestFixture, RequiresSilo]
            [Category("Slow")]
            class Tests
            {
                static TestCases<TestConsumerActor> Verify() => 
                   new TestCases<TestConsumerActor>("aqp", TimeSpan.FromSeconds(5));

                [Test] public async Task Resuming_on_reactivation()                         => await Verify().Resuming_on_reactivation();
                [Test] public async Task Subscription_is_idempotent()                       => await Verify().Subscription_is_idempotent();
                [Test] public async Task Select_all_filter()                                => await Verify().Select_all_filter();
                [Test] public async Task Explicit_filter()                                  => await Verify().Explicit_filter();
            }
        }
    }
}