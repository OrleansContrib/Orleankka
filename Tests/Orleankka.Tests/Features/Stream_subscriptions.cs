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
        {
            public StreamFilter Filter;
        }

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

            Task On(Subscribe x) => Stream().Subscribe(this, x.Filter);
            void On(Kill x) => DeactivateOnIdle();

            void On(string x) => received.Add(x);
            List<string> On(Received x) => received;

            Task On(Activate _) => Stream().Resume(this);

            StreamRef Stream() => System.StreamOf("sms", "sms-42");

            public override async Task<object> Receive(object message)
            {
                switch (message)
                {
                    case int x:
                        received.Add(x.ToString());
                        break;
                    default:
                        return await base.Receive(message);
                }

                return Done;
            }
        }

        [TestFixture] 
        [RequiresSilo]
        class Tests
        {
            static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);
            IActorSystem system;

            [SetUp] public void SetUp() => 
                system = TestActorSystem.Instance;

            [Test, Category("Slow")]
            public async Task Resuming_on_reactivation()
            {
                var consumer = system.ActorOf<ITestConsumerActor>("cons");
                await consumer.Tell(new Subscribe());

                var stream = system.StreamOf("sms", "sms-42");
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

                var stream = system.StreamOf("sms", "sms-42");
                await stream.Publish("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
            }

            [Test] 
            public async Task Select_all_filter()
            {
                var consumer = system.ActorOf<ITestConsumerActor>("select-all");
                await consumer.Tell(new Subscribe {Filter = StreamFilter.ReceiveAll});

                var stream = system.StreamOf("sms", "sms-42");
                await stream.Publish(123);
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("123"));
            }

            [Test] 
            public async Task Explicit_filter()
            {
                var consumer = system.ActorOf<ITestConsumerActor>("fff");

                var filter = new StreamFilter(DropAll);
                await consumer.Tell(new Subscribe { Filter = filter });

                var stream = system.StreamOf("sms", $"{"sms"}-filtered");
                await stream.Publish("e-123");
                await Task.Delay(timeout);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(0));
            }

            static bool DropAll(object item) => false;
        }
    }
}