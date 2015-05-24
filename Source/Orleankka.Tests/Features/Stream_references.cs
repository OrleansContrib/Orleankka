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
        using Typed;
        using Testing;

        public class TestProducerActor : TypedActor
        {
            public Task Produce(string e)
            {
                var stream = System.StreamOf<SimpleMessageStreamProvider>("123");
                return stream.OnNextAsync(e);
            }
        }

        public class TestConsumerActor : TypedActor
        {
            readonly TestStreamObserver observer = new TestStreamObserver();
            StreamSubscriptionHandle<object> subscription;

            public async Task Subscribe()
            {
                var stream = System.StreamOf<SimpleMessageStreamProvider>("123");
                subscription = await stream.SubscribeAsync(observer);
            }

            public List<object> Received
            {
                get { return observer.Received; }
            }
        }

        class TestStreamObserver : IAsyncObserver<object>
        {
            public readonly List<object> Received = new List<object>();

            public Task OnNextAsync(object item, StreamSequenceToken token = null)
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
                var producer = system.TypedActorOf<TestProducerActor>("p");
                var consumer = system.TypedActorOf<TestConsumerActor>("c");

                await consumer.Call(x => x.Subscribe());
                await producer.Call(x => x.Produce("event"));

                await Task.Delay(100);
                var received = await consumer.Get(x => x.Received);
                
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("event"));
            }
        }
    }
}