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
        class Produce : Command
        {
            public StreamRef Stream;
            public Item Item;
        }

        [Serializable]
        class Item
        {
            public readonly string Text;

            public Item(string text)
            {
                Text = text;
            }
        }

        [Serializable]
        class Subscribe : Command
        {
            public StreamRef Stream;
        }

        [Serializable]
        class Unsubscribe : Command
        {
            public StreamRef Stream;
        }

        [Serializable]
        class Received : Query<List<Item>>
        {}

        abstract class TestProducerActorBase : Actor
        {
            Task On(Produce x) => x.Stream.Push(x.Item);
        }

        abstract class TestConsumerActorBase : Actor
        {
            readonly List<Item> received = new List<Item>();

            Task On(Subscribe x) => x.Stream.Subscribe(this);
            Task On(Unsubscribe x) => x.Stream.Unsubscribe(this);

            void On(Item x) => received.Add(x);
            List<Item> On(Received x) => received;
        }

        abstract class Tests<TProducer, TConsumer> 
            where TProducer : TestProducerActorBase 
            where TConsumer : TestConsumerActorBase
        {
            IActorSystem system;

            [SetUp]
            public void SetUp() => system = TestActorSystem.Instance;

            [Test]
            public async void Client_to_stream()
            {
                var stream = system.StreamOf(Provider, $"{Provider}-123");

                var received = new List<Item>();
                var subscription = await stream.Subscribe<Item>(
                    item => received.Add(item));

                await stream.Push(new Item("foo"));
                await Task.Delay(Timeout);

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await subscription.Unsubscribe();
                received.Clear();

                await stream.Push("bar");
                await Task.Delay(Timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }

            [Test]
            public async void Actor_to_stream()
            {
                var stream = system.StreamOf(Provider, $"{Provider}-123");

                var producer = system.ActorOf<TProducer>("p");
                var consumer = system.ActorOf<TConsumer>("c");

                await consumer.Tell(new Subscribe {Stream = stream});
                await producer.Tell(new Produce {Stream = stream, Item = new Item("foo")});

                await Task.Delay(Timeout);
                var received = await consumer.Ask(new Received());

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await consumer.Tell(new Unsubscribe {Stream = stream});
                received.Clear();

                await producer.Tell(new Produce {Stream = stream, Item = new Item("bar")});
                await Task.Delay(Timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }

            protected abstract string Provider  { get; }
            protected abstract TimeSpan Timeout { get; }
        }

        namespace SimpleMessageStreamProviderVerification
        {
            class TestProducerActor : TestProducerActorBase {}
            class TestConsumerActor : TestConsumerActorBase {}

            [TestFixture, RequiresSilo]
            class Tests : Tests<TestProducerActor, TestConsumerActor>
            {
                protected override string Provider  => "sms";
                protected override TimeSpan Timeout => TimeSpan.FromMilliseconds(100);
            }
        }

        namespace AzureQueueStreamProviderVerification
        {
            class TestProducerActor : TestProducerActorBase { }
            class TestConsumerActor : TestConsumerActorBase { }

            [TestFixture, RequiresSilo, Category("Slow"), Explicit]
            class Tests : Tests<TestProducerActor, TestConsumerActor>
            {
                protected override string Provider  => "aqp";
                protected override TimeSpan Timeout => TimeSpan.FromMilliseconds(1000);
            }
        }
    }
}