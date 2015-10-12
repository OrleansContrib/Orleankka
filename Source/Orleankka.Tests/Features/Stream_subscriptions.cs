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
        {}

        [Serializable]
        public class Deactivate : Command
        {}

        [Serializable]
        public class Received : Query<List<string>>
        {}

        abstract class TestConsumerActorBase : Actor
        {
            readonly List<string> received = new List<string>();

            Task On(Subscribe x) => Stream().Subscribe(this);
            void On(Deactivate x) => Activation.DeactivateOnIdle();

            void On(string x) => received.Add(x);
            List<string> On(Received x) => received;

            public override Task OnActivate() => Stream().Resume(this);

            StreamRef Stream() => System.StreamOf(Provider, $"{Provider}-42");
            protected abstract string Provider { get; }
        }

        abstract class TestSubscriptionHandlesActorBase : Actor
        {
            public override Task OnActivate() => Stream().Subscribe(this);
            async Task<int> On(string x) => (await Stream().GetAllSubscriptionHandles()).Count;

            StreamRef Stream() => System.StreamOf(Provider, $"{Provider}-subs");
            protected abstract string Provider { get; }
        }

        abstract class Tests<TConsumer, TSubscriptions>
            where TConsumer : TestConsumerActorBase
            where TSubscriptions : TestSubscriptionHandlesActorBase
        {
            IActorSystem system;

            [SetUp]
            public void SetUp() => system = TestActorSystem.Instance;

            [Test]
            public async void Get_all_subscription_handles()
            {
                var a1 = system.ActorOf<TSubscriptions>("123");
                var a2 = system.ActorOf<TSubscriptions>("456");

                Assert.That((await a1.Ask<int>("count")), Is.EqualTo(1),
                            "Should return handles registered only by the current actor");

                Assert.That((await a2.Ask<int>("count")), Is.EqualTo(1),
                            "Should return handles registered only by the current actor");

                var stream = system.StreamOf(Provider, $"{Provider}-subs");
                Assert.That((await stream.GetAllSubscriptionHandles()).Count, Is.EqualTo(0),
                            "Should not return all registered handles when requested from the client side");
            }

            [Test]
            public async void Resuming_on_reactivation()
            {
                var consumer = system.ActorOf<TConsumer>("cons");
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf(Provider, $"{Provider}-42");
                await stream.Push("e-123");
                await Task.Delay(Timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-123"));

                await consumer.Tell(new Deactivate());
                await Task.Delay(TimeSpan.FromSeconds(61));

                await stream.Push("e-456");
                await Task.Delay(Timeout);

                received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-456"));
            }

            [Test]
            public async void Subscription_is_idempotent()
            {
                var consumer = system.ActorOf<TConsumer>("idempotent");

                await consumer.Tell(new Subscribe());
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf(Provider, $"{Provider}-42");
                await stream.Push("e-123");
                await Task.Delay(Timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
            }

            protected abstract string Provider  { get; }
            protected abstract TimeSpan Timeout { get; }
        }

        namespace SimpleMessageStreamProviderVerification
        {
            class TestConsumerActor : TestConsumerActorBase
            {
                protected override string Provider => "sms";
            }

            class TestSubscriptionHandlesActor : TestSubscriptionHandlesActorBase
            {
                protected override string Provider => "sms";
            }

            [TestFixture, RequiresSilo]
            class Tests : Tests<TestConsumerActor, TestSubscriptionHandlesActor>
            {
                protected override string Provider  => "sms";
                protected override TimeSpan Timeout => TimeSpan.FromMilliseconds(100);
            }
        }

        namespace AzureQueueStreamProviderVerification
        {
            class TestConsumerActor : TestConsumerActorBase
            {
                protected override string Provider => "aqp";
            }

            class TestSubscriptionHandlesActor : TestSubscriptionHandlesActorBase
            {
                protected override string Provider => "aqp";
            }

            [TestFixture, RequiresSilo, Category("Slow"), Explicit]
            class Tests : Tests<TestConsumerActor, TestSubscriptionHandlesActor>
            {
                protected override string Provider  => "aqp";
                protected override TimeSpan Timeout => TimeSpan.FromSeconds(5);
            }
        }
    }
}