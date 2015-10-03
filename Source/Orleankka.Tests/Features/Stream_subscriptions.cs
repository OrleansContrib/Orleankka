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

            async Task On(Subscribe x)   => await System.StreamOf("sms", "42")
                 .Subscribe<string>(item => received.Add(item));

            List<string> On(Received x)  => received;
            void On(Deactivate x)        => Activation.DeactivateOnIdle();
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
        }
    }
}