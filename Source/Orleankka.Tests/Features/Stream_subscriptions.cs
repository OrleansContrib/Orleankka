using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;
using Orleans.Streams;
using Orleans.Providers.Streams.SimpleMessageStream;

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
            readonly TestStreamObserver observer = new TestStreamObserver();
            StreamRef Stream() => System.StreamOf<SimpleMessageStreamProvider>("42");

            public override async Task OnActivate()
            {
                var subscriptions = await Stream().GetAllSubscriptionHandles();

                if (subscriptions.Count != 0)
                    await subscriptions[0].ResumeAsync(observer);
            }

            public async Task On(Subscribe x)   => await Stream().SubscribeAsync(observer);
            public List<string> On(Received x)  => observer.Received;
            public void On(Deactivate x)        => Activation.DeactivateOnIdle();
        }

        class TestStreamObserver : IAsyncObserver<string>
        {
            public readonly List<string> Received = new List<string>();

            public Task OnNextAsync(string item, StreamSequenceToken token = null)
            {
                Received.Add(item);
                return TaskDone.Done;
            }

            public Task OnCompletedAsync()
            {
                return TaskDone.Done;
            }

            public Task OnErrorAsync(Exception ex)
            {
                return TaskDone.Done;
            }
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

                var stream = system.StreamOf<SimpleMessageStreamProvider>("42");
                await stream.OnNextAsync("e-123");
                await Task.Delay(100);

                var received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-123"));

                await consumer.Tell(new Deactivate());
                await Task.Delay(TimeSpan.FromSeconds(61));

                await stream.OnNextAsync("e-456");
                await Task.Delay(1000);

                received = await consumer.Ask(new Received());
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("e-456"));
            }
        }
    }
}