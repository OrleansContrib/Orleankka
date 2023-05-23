using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

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
        public class Kill : Command
        {}

        [Serializable]
        public class Received : Query<List<string>>
        {}

        public interface ITestConsumerActor : IActorGrain, IGrainWithStringKey
        {}

        public class TesITestConsumerActorActor : DispatchActorGrain, ITestConsumerActor
        {
            readonly List<string> received = new List<string>();

            Task On(Subscribe x) => Stream().Subscribe(this, new SubscribeReceiveItem());
            void On(Kill x) => DeactivateOnIdle();

            void On(StreamItem<object> x) => received.Add(x.Item.ToString());
            List<string> On(Received x) => received;

            Task On(Activate _) => Stream().Resume(this);
            StreamRef<object> Stream() => System.StreamOf<object>("sms", "sms-42");
        }

        [TestFixture] 
        [RequiresSilo]
        class Tests
        {
            static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(1000);
            IActorSystem system;

            [SetUp] public void SetUp() => 
                system = TestActorSystem.Instance;

            [Test, Category("Slow")]
            public async Task Resuming_on_reactivation()
            {
                var consumer = system.ActorOf<ITestConsumerActor>("cons");
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf<object>("sms", "sms-42");
                await stream.Publish("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-123"));

                await consumer.Tell(new Kill());
                await Task.Delay(TimeSpan.FromSeconds(61));

                await stream.Publish("e-456");
                await Task.Delay(timeout);

                received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-456"));
            }

            [Test] 
            public async Task Subscription_is_idempotent()
            {
                var consumer = system.ActorOf<ITestConsumerActor>("idempotent");

                await consumer.Tell(new Subscribe());
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf<object>("sms", "sms-42");
                await stream.Publish("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
            }
        }
    }
}