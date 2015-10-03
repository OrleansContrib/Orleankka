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
            public string Event;
        }

        [Serializable]
        public class Subscribe : Command
        {
            public StreamRef Stream;
        }

        [Serializable]
        public class Received : Query<List<string>>
        {}

        class TestProducerActor : Actor
        {
            Task On(Produce cmd)
            {
                var stream = System.StreamOf("sms", "123");
                return stream.Push(cmd.Event);
            }
        }

        class TestConsumerActor : Actor
        {
            readonly List<string> received = new List<string>();

            Task On(Subscribe x) => x.Stream.Subscribe<string>(
                item => received.Add(item));

            List<string> On(Received x) => received;
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

                var received = new List<string>();
                await stream.Subscribe<string>(
                    item => received.Add(item));

                await stream.Push("event");
                await Task.Delay(100);

                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("event"));
            }

            [Test]
            public async void Actor_to_stream()
            {
                var stream = system.StreamOf("sms", "123");

                var producer = system.ActorOf<TestProducerActor>("p");
                var consumer = system.ActorOf<TestConsumerActor>("c");

                await consumer.Tell(new Subscribe {Stream = stream});
                await producer.Tell(new Produce {Event = "event"});

                await Task.Delay(100);
                var received = await consumer.Ask(new Received());
                
                Assert.That(received.Count, Is.EqualTo(1));
                Assert.That(received[0], Is.EqualTo("event"));
            }
        }
    }
}