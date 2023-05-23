using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;
using Orleans.Metadata;

namespace Orleankka.Features
{
    namespace Stream_references
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Produce : Command
        {
            public StreamRef<ItemData> Stream;
            public ItemData ItemData;
        }

        [Serializable, GenerateSerializer]
        public class ItemData
        {
            [Id(0)] public readonly string Text;

            public ItemData(string text)
            {
                Text = text;
            }

            public static bool DropAll(object item) => false;
        }

        [Serializable]
        public class Subscribe : Command
        {
            public StreamRef<ItemData> Stream;
        }

        [Serializable]
        public class Unsubscribe : Command
        {
            public StreamRef<ItemData> Stream;
        }

        [Serializable]
        class Received : Query<List<ItemData>>
        {}
        
        [DefaultGrainType("test-stream-refs-producer")]
        public interface ITestProducerActor : IActorGrain, IGrainWithStringKey {}

        [DefaultGrainType("test-stream-refs-consumer")]
        public interface ITestConsumerActor : IActorGrain, IGrainWithStringKey {}

        [GrainType("test-stream-refs-producer")]
        public class TestProducerActor : DispatchActorGrain, ITestProducerActor
        {
            Task On(Produce x) => x.Stream.Publish(x.ItemData);
        }

        [GrainType("test-stream-refs-consumer")]
        public class TestConsumerActor : DispatchActorGrain, ITestConsumerActor
        {
            readonly List<ItemData> received = new List<ItemData>();

            Task On(Subscribe x) => x.Stream.Subscribe(this);
            Task On(Unsubscribe x) => x.Stream.Unsubscribe(this);

            void On(StreamItem<ItemData> x) => received.Add(x.Item);
            List<ItemData> On(Received x) => received;
        }

        class Tests
        {
            static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(100);
            IActorSystem system;

            [SetUp] public void SetUp() => 
                system = TestActorSystem.Instance;

            [Test]
            public async Task Client_to_stream()
            {
                var stream = system.StreamOf<ItemData>("sms", "sms-123");

                var received = new List<ItemData>();
                var subscription = await stream.Subscribe(
                    (item, _) => received.Add(item));

                await stream.Publish(new ItemData("foo"));
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await subscription.Unsubscribe();
                received.Clear();

                await stream.Publish(new ItemData("bar"));
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }

            [Test]
            public async Task Actor_to_stream()
            {
                var stream = system.StreamOf<ItemData>("sms", "sms-123");

                var producer = system.ActorOf<ITestProducerActor>("p");
                var consumer = system.ActorOf<ITestConsumerActor>("c");

                await consumer.Tell(new Subscribe {Stream = stream});
                await producer.Tell(new Produce {Stream = stream, ItemData = new ItemData("foo")});

                await Task.Delay(timeout);
                var received = await consumer.Ask(new Received());

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await consumer.Tell(new Unsubscribe {Stream = stream});
                received.Clear();

                await producer.Tell(new Produce {Stream = stream, ItemData = new ItemData("bar")});
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }
        }
    }
}