using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Stream_references
    {
        using Meta;
        using Testing;

        [Serializable]
        public class Produce : Command
        {
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
        public class Received : Query<List<Item>>
        {}

        class TestProducerActor : Actor
        {
            Task On(Produce cmd)
            {
                var stream = System.StreamOf("sms", "123");
                return stream.Push(cmd.Item);
            }
        }

        class TestConsumerActor : Actor
        {
            readonly List<Item> received = new List<Item>();

            Task On(Subscribe x) => x.Stream.Subscribe(this);
            Task On(Unsubscribe x) => x.Stream.Unsubscribe(this);

            void On(Item x) => received.Add(x);
            List<Item> On(Received x) => received;
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
                var stream = system.StreamOf("sms", "123");

                var received = new List<Item>();
                var subscription = await stream.Subscribe<Item>(
                    item => received.Add(item));

                await stream.Push(new Item("foo"));
                await Task.Delay(100);

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await subscription.Unsubscribe();
                received.Clear();

                await stream.Push("bar");
                await Task.Delay(100);

                Assert.That(received.Count, Is.EqualTo(0));
            }

            [Test]
            public async void Actor_to_stream()
            {
                var stream = system.StreamOf("sms", "123");

                var producer = system.ActorOf<TestProducerActor>("p");
                var consumer = system.ActorOf<TestConsumerActor>("c");

                await consumer.Tell(new Subscribe {Stream = stream});
                await producer.Tell(new Produce {Item = new Item("foo")});

                await Task.Delay(100);
                var received = await consumer.Ask(new Received());
                
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await consumer.Tell(new Unsubscribe {Stream = stream}); ;
                received.Clear();

                await producer.Tell(new Produce {Item = new Item("bar")});
                await Task.Delay(100);

                Assert.That(received.Count, Is.EqualTo(0));
            }
        }
    }
}