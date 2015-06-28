using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;
using Orleans.Streams;
using Orleans.Providers.Streams.SimpleMessageStream;

namespace Orleankka.Features
{
    namespace Stream_references
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Produce : Command
        {
            public string Event;
        }

        [Serializable]
        public class Subscribe : Command
        {}

        [Serializable]
        public class Received : Query<List<string>>
        {}

        public class TestProducerActor : UntypedActor
        {
            public Task Handle(Produce cmd)
            {
                var stream = System.StreamOf<SimpleMessageStreamProvider>("123");
                return stream.OnNextAsync(cmd.Event);
            }
        }

        public class TestConsumerActor : UntypedActor
        {
            readonly TestStreamObserver observer = new TestStreamObserver();
            StreamSubscriptionHandle<object> subscription;

            protected internal override void Define()
            {
                On(async (Subscribe x) =>
                {
                    var stream = System.StreamOf<SimpleMessageStreamProvider>("123");
                    subscription = await stream.SubscribeAsync(observer);
                });

                On((Received x) => observer.Received);
            }
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
        [RequiresSilo]
        public class Tests
        {
            IActorSystem system;

            [SetUp]
            public void SetUp()
            {
                system = TestActorSystem.Instance;
            }

            [Test]
            public async void Client_to_stream()
            {
                var stream = system.StreamOf<SimpleMessageStreamProvider>("123");
                
                var observer = new TestStreamObserver();
                await stream.SubscribeAsync(observer);

                await stream.OnNextAsync("event");
                await Task.Delay(100);

                Assert.That(observer.Received.Count, Is.EqualTo(1));
                Assert.That(observer.Received[0], Is.EqualTo("event"));
            }

            [Test]
            public async void Actor_to_stream()
            {
                var producer = system.ActorOf<TestProducerActor>("p");
                var consumer = system.ActorOf<TestConsumerActor>("c");

                await consumer.Tell(new Subscribe());
                await producer.Tell(new Produce {Event = "event"});

                await Task.Delay(100);
                var received = await consumer.Ask(new Received());
                
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("event"));
            }
        }
    }
}