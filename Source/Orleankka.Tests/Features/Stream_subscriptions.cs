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

        public class TestConsumerActor : Actor
        {
            readonly List<string> received = new List<string>();

            Task On(Subscribe x) => Stream().Subscribe(this);
            void On(Deactivate x) => Activation.DeactivateOnIdle();

            void On(string x) => received.Add(x);
            List<string> On(Received x) => received;

            StreamRef Stream() => System.StreamOf("sms", "42");
            public override Task OnActivate() => Stream().Resume(this);
        }

        class TestSubscriptionHandlesActor : Actor
        {
            StreamRef Stream() => System.StreamOf("sms", "111");

            public override Task OnActivate() => Stream().Subscribe(this);
            
            async Task<int> On(string x) => (await Stream().GetAllSubscriptionHandles()).Count;
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
            public async void Learn_about_get_all_subscription_handles()
            {
                var a1 = system.ActorOf<TestSubscriptionHandlesActor>("123");
                var a2 = system.ActorOf<TestSubscriptionHandlesActor>("456");

                Assert.That((await a1.Ask<int>("count")), Is.EqualTo(1), 
                    "Should return handles registered only by the current actor");

                Assert.That((await a2.Ask<int>("count")), Is.EqualTo(1),
                    "Should return handles registered only by the current actor");

                var stream = system.StreamOf("sms", "111");
                Assert.That((await stream.GetAllSubscriptionHandles()).Count, Is.EqualTo(0),
                    "Should not return all registered handles when requested from the client side");
            }

            [Test]
            public async void Resuming_on_reactivation()
            {
                var consumer = system.ActorOf<TestConsumerActor>("cons");
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf("sms", "42");
                await stream.Push("e-123");
                await Task.Delay(100);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-123"));

                await consumer.Tell(new Deactivate());
                await Task.Delay(TimeSpan.FromSeconds(61));

                await stream.Push("e-456");
                await Task.Delay(1000);

                received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-456"));
            }

            [Test]
            public async void Subscription_is_idempotent()
            {
                var consumer = system.ActorOf<TestConsumerActor>("idempotent");

                await consumer.Tell(new Subscribe());
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf("sms", "42");
                await stream.Push("e-123");
                await Task.Delay(100);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
            }
        }
    }
}