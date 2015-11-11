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
        public class Deactivate : Command
        {}

        [Serializable]
        public class Received : Query<List<string>>
        {}

        abstract class TestConsumerActorBase : Actor
        {
            readonly List<string> received = new List<string>();

            Task On(Subscribe x) => Stream().Subscribe(this, x.Filter);
            void On(Deactivate x) => Activation.DeactivateOnIdle();

            void On(string x) => received.Add(x);
            List<string> On(Received x) => received;

            public override Task OnActivate() => Stream().Resume(this);

            StreamRef Stream() => System.StreamOf(Provider, $"{Provider}-42");
            protected abstract string Provider { get; }
        }

        class TestCases<TConsumer> where TConsumer : IActor
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

                await consumer.Tell(new Deactivate());
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

            static bool DropAll(object item) => false;

            public async Task Filtering_items()
            {
                var consumer = system.ActorOf<TConsumer>("fff");

                var filter = new StreamFilter(DropAll);
                await consumer.Tell(new Subscribe {Filter = filter});

                var stream = system.StreamOf(provider, $"{provider}-filtered");
                await stream.Push("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(0));

                await consumer.Tell(new Deactivate());
                await Task.Delay(TimeSpan.FromSeconds(61));

                await stream.Push("e-456");
                await Task.Delay(timeout);

                received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(0));
            }
        }

        namespace SimpleMessageStreamProviderVerification
        {
            class TestConsumerActor : TestConsumerActorBase
            {
                protected override string Provider => "sms";
            }

            [TestFixture, RequiresSilo]
            class Tests
            {
                static TestCases<TestConsumerActor> Verify() => 
                   new TestCases<TestConsumerActor>("sms", TimeSpan.FromMilliseconds(100));

                [Test] public async Task Resuming_on_reactivation()     => await Verify().Resuming_on_reactivation();
                [Test] public async Task Subscription_is_idempotent()   => await Verify().Subscription_is_idempotent();
                [Test] public async Task Filtering_items()              => await Verify().Filtering_items();
            }
        }

        namespace AzureQueueStreamProviderVerification
        {
            class TestConsumerActor : TestConsumerActorBase
            {
                protected override string Provider => "aqp";
            }

            [TestFixture]
            [RequiresSilo(Fresh = true, EnableAzureQueueStreamProvider = true)]
            [Category("Slow"), Explicit]
            class Tests
            {
                static TestCases<TestConsumerActor> Verify() => 
                   new TestCases<TestConsumerActor>("aqp", TimeSpan.FromSeconds(5));

                [Test] public async Task Resuming_on_reactivation()     => await Verify().Resuming_on_reactivation();
                [Test] public async Task Subscription_is_idempotent()   => await Verify().Subscription_is_idempotent();
                [Test] public async Task Filtering_items()              => await Verify().Filtering_items();
            }
        }
    }
}