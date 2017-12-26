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

        public abstract class TestProducerActorBase : ActorGrain
        {
            Task On(Produce x) => x.Stream.Push(x.Item);
        }

        public abstract class TestConsumerActorBase : ActorGrain
        {
            readonly List<Item> received = new List<Item>();

            Task On(Subscribe x) => x.Stream.Subscribe(this);
            Task On(Unsubscribe x) => x.Stream.Unsubscribe(this);

            void On(Item x) => received.Add(x);
            List<Item> On(Received x) => received;
        }

        class TestCases<TProducer, TConsumer> 
            where TProducer : TestProducerActorBase, IActorGrain 
            where TConsumer : TestConsumerActorBase, IActorGrain
        {
            readonly string provider;
            readonly TimeSpan timeout;
            readonly IActorSystem system;

            public TestCases(string provider, TimeSpan timeout)
            {
                this.provider = provider;
                this.timeout = timeout;
                system = TestActorSystem.Instance;
            }

            public async Task Client_to_stream()
            {
                var stream = system.StreamOf(provider, $"{provider}-123");

                var received = new List<Item>();
                var subscription = await stream.Subscribe<Item>(
                    item => received.Add(item));

                await stream.Push(new Item("foo"));
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0].Text, Is.EqualTo("foo"));

                await subscription.Unsubscribe();
                received.Clear();

                await stream.Push("bar");
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }

            public async Task Actor_to_stream()
            {
                var stream = system.StreamOf(provider, $"{provider}-123");

                var producer = system.ActorOf<TProducer>("p");
                var consumer = system.ActorOf<TConsumer>("c");

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

            public async Task Filtering_items()
            {
                var stream = system.StreamOf(provider, $"{provider}-fff");

                var received = new List<Item>();
                await stream.Subscribe<Item>(
                    callback: item => received.Add(item),
                    filter: new StreamFilter(Item.DropAll));

                await stream.Push(new Item("foo"));
                await Task.Delay(timeout);

                Assert.That(received.Count, Is.EqualTo(0));
            }
        }

        namespace SimpleMessageStreamProviderVerification
        {
            public interface ITestProducerActor : IActorGrain {}
            public interface ITestConsumerActor : IActorGrain {}

            public class TestProducerActor : TestProducerActorBase, ITestProducerActor {}
            public class TestConsumerActor : TestConsumerActorBase, ITestConsumerActor {}

            [TestFixture, RequiresSilo]
            class Tests
            {
                static TestCases<TestProducerActor, TestConsumerActor> Verify() =>
                   new TestCases<TestProducerActor, TestConsumerActor>("sms", TimeSpan.FromMilliseconds(100));

                [Test] public async Task Client_to_stream() => await Verify().Client_to_stream();
                [Test] public async Task Actor_to_stream()  => await Verify().Actor_to_stream();
                [Test] public async Task Filtering_items()  => await Verify().Filtering_items();
            }
        }

        namespace AzureQueueStreamProviderVerification
        {
            public interface ITestProducerActor : IActorGrain {}
            public interface ITestConsumerActor : IActorGrain {}

            public class TestProducerActor : TestProducerActorBase, ITestProducerActor {}
            public class TestConsumerActor : TestConsumerActorBase, ITestConsumerActor {}

            [TestFixture, RequiresSilo]
            [Category("Slow")]
            class Tests
            {
                static TestCases<TestProducerActor, TestConsumerActor> Verify() =>
                   new TestCases<TestProducerActor, TestConsumerActor>("aqp", TimeSpan.FromSeconds(5));

                [Test] public async Task Client_to_stream() => await Verify().Client_to_stream();
                [Test] public async Task Actor_to_stream()  => await Verify().Actor_to_stream();
                [Test] public async Task Filtering_items()  => await Verify().Filtering_items();
            }
        }
    }
}