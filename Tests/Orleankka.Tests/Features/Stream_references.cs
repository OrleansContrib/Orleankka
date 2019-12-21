using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Features
{
    namespace Stream_references
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Produce : Command
        {
            public StreamRef Stream;
            public Item Item;
        }

        [Serializable]
        public class Item
        {
            public readonly string Text;

            public Item(string text)
            {
                Text = text;
            }

            public static bool DropAll(object item) => false;
        }

        [Serializable]
        public class Subscribe : Command
        {
            public StreamRef Stream;
        }

        [Serializable]
        public class Unsubscribe : Command
        {
            public StreamRef Stream;
        }

        [Serializable]
        class Received : Query<List<Item>>
        {}

        public interface ITestProducerActor : IActorGrain, IGrainWithStringKey {}
        public interface ITestConsumerActor : IActorGrain, IGrainWithStringKey {}

        public class TestProducerActor : DispatchActorGrain, ITestProducerActor
        {
            Task On(Produce x) => x.Stream.Publish(x.Item);
        }

        public class TestConsumerActor : DispatchActorGrain, ITestConsumerActor
        {
            readonly List<Item> received = new List<Item>();

            Task On(Subscribe x) => x.Stream.Subscribe(this);
            Task On(Unsubscribe x) => x.Stream.Unsubscribe(this);

            void On(Item x) => received.Add(x);
            List<Item> On(Received x) => received;
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
                var stream = system.StreamOf("sms", "sms-123");

                var received = new List<Item>();
                var subscription = await stream.Subscribe<Item>(
                    item => received.Add(item));

                await stream.Publish(new Item("foo"));
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await subscription.Unsubscribe();
                received.Clear();

                await stream.Publish("bar");
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }

            [Test]
            public async Task Actor_to_stream()
            {
                var stream = system.StreamOf("sms", "sms-123");

                var producer = system.ActorOf<ITestProducerActor>("p");
                var consumer = system.ActorOf<ITestConsumerActor>("c");

                await consumer.Tell(new Subscribe {Stream = stream});
                await producer.Tell(new Produce {Stream = stream, Item = new Item("foo")});

                await Task.Delay(timeout);
                var received = await consumer.Ask(new Received());

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await consumer.Tell(new Unsubscribe {Stream = stream});
                received.Clear();

                await producer.Tell(new Produce {Stream = stream, Item = new Item("bar")});
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }

            [Test]
            public async Task Filtering_items()
            {
                var stream = system.StreamOf("sms", "sms-fff");

                var received = new List<Item>();
                await stream.Subscribe<Item>(
                    callback: item => received.Add(item),
                    filter: new StreamFilter(Item.DropAll));

                await stream.Publish(new Item("foo"));
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }
        }
    }
}