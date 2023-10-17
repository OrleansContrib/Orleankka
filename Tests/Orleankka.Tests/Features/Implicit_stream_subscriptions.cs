using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleankka.Features
{
    namespace Implicit_stream_subscriptions
    {
        using Meta;
        using NUnit.Framework;
        using Orleans;

        using Testing;
        
        public record CreateMessage : Command;
        public record MessageCreated : Event;
        public record GetMessages : Query<List<object>>;

        public interface ITestProducerActor : IActorGrain, IGrainWithStringKey
        { }

        public interface ITestConsumerActor : IActorGrain, IGrainWithStringKey
        { }

        public class TestProducerActor : DispatchActorGrain, ITestProducerActor
        {
            string SelfStreamPath() => $"{nameof(TestProducerActor)}:{GrainReference.GrainId.Key}-Events";
            StreamRef<object> SelfStream() => System.StreamOf<object>("sms", SelfStreamPath());
            Task Handle(CreateMessage c) => SelfStream().Publish(new MessageCreated());
        }

        // Example of what we did before:
        // Filter by {provider}:/^{actor_name}:{actor_id}-{stream_type}/
        // Possibility to select the actor `Id` with `Target`
        // Possibility to provider a `Filter` for messages
        // [StreamSubscription(Source = "sms:/^TestProducerActor:(?<id>.*)-Events/", Target = "id", Filter = null)]
        // [StreamSubscription(Source = "sms:/^TestProducerActor:(?<id>.*)-Events/", Target = "{id}", Filter = "*")]
        // [StreamSubscription(Source = "sms:/^TestProducerActor:(?<id>.*)-Events/", Target = "Correlate()", Filter = "Filter()")]
        // how can we achieve this now?
        [ImplicitStreamSubscription]
        public class TestConsumerActor : DispatchActorGrain, ITestConsumerActor
        {
            readonly List<object> received = new List<object>();

            public static string Correlate(object item) => item is MessageCreated ? "id" : "other_id";
            public static bool Filter(object item) => item is MessageCreated;

            void On(StreamItem<object> x) => received.Add(x.Item);
            List<object> On(GetMessages x) => received;
        }

        [TestFixture]
        [RequiresSilo]
        class Tests
        {
            static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(1000);
            IActorSystem system;

            [SetUp]
            public void SetUp() =>
                system = TestActorSystem.Instance;

            [Test]
            public async Task Implicit_stream_subscription()
            {
                var publisher = system.ActorOf<ITestProducerActor>("id");
                await publisher.Tell(new CreateMessage());

                await Task.Delay(timeout);
                
                var consumer = system.ActorOf<ITestConsumerActor>("id");
                var received = await consumer.Ask(new GetMessages());

                Assert.That(received.Count, Is.EqualTo(1));
            }
        }
    }
}
