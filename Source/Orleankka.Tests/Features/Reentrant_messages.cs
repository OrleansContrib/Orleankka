using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Reentrant_messages
    {
        using Meta;
        using Testing;

        [Serializable]
        class NonReentrantMessage : Query<ActorState>
        {
            public int Id;
            public TimeSpan Delay;
        }

        [Serializable]
        class ReentrantMessage : Query<ActorState>
        {
            public int Id;
            public TimeSpan Delay;
        }

        [Serializable]
        class ActorState
        {
            public readonly List<int> ReentrantInProgress = new List<int>();
            public readonly List<int> NonReentrantInProgress = new List<int>();
        }

        [Reentrant(typeof(ReentrantMessage))]
        class TestActor : Actor
        {
            readonly ActorState state = new ActorState();

            async Task On(NonReentrantMessage x)
            {
                if (state.NonReentrantInProgress.Count > 0)
                    throw new InvalidOperationException("Can't be interleaved");

                state.NonReentrantInProgress.Add(x.Id);
                await Task.Delay(x.Delay);

                state.NonReentrantInProgress.Remove(x.Id);
            }

            async Task<ActorState> On(ReentrantMessage x)
            {
                state.ReentrantInProgress.Add(x.Id);
                await Task.Delay(x.Delay);

                state.ReentrantInProgress.Remove(x.Id);
                return state;
            }
        }

        [Serializable] class Activate : Command {}
        [Serializable] class GetStreamMessagesInProgress : Query<List<object>> {}

        [Reentrant(typeof(GetStreamMessagesInProgress))]
        [Reentrant(typeof(int))]   // 1-st stream message type        
        class TestReentrantStreamConsumerActor : Actor
        {
            readonly List<object> streamMessagesInProgress = new List<object>();
            List<object> On(GetStreamMessagesInProgress x) => streamMessagesInProgress;

            async Task On(Activate x)
            {
                var stream1 = System.StreamOf("sms", "s1");
                var stream2 = System.StreamOf("sms", "s2");

                await stream1.Subscribe<int>(item =>
                {
                    streamMessagesInProgress.Add(item);
                    return Task.Delay(500);
                });

                await stream2.Subscribe<string>(item =>
                {
                    streamMessagesInProgress.Add(item);
                    return Task.Delay(500);
                });
            }
        }

        [Reentrant(nameof(IsReentrant))]
        class TestReentrantByCallbackMethodActor : Actor
        {
            public static bool IsReentrant(object msg) => msg is ReentrantMessage;

            readonly ActorState state = new ActorState();

            async Task On(NonReentrantMessage x)
            {
                if (state.NonReentrantInProgress.Count > 0)
                    throw new InvalidOperationException("Can't be interleaved");

                state.NonReentrantInProgress.Add(x.Id);
                await Task.Delay(x.Delay);

                state.NonReentrantInProgress.Remove(x.Id);
            }

            async Task<ActorState> On(ReentrantMessage x)
            {
                state.ReentrantInProgress.Add(x.Id);
                await Task.Delay(x.Delay);

                state.ReentrantInProgress.Remove(x.Id);
                return state;
            }
        }

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
            public async Task When_reentrant_determined_by_message_type()
            {
                var actor = system.FreshActorOf<TestActor>();
                await TestReentrantReceive(actor);
            }

            [Test]
            public async Task When_reentrant_determined_by_callback_method()
            {
                var actor = system.FreshActorOf<TestReentrantByCallbackMethodActor>();
                await TestReentrantReceive(actor);
            }

            static async Task TestReentrantReceive(ActorRef actor)
            {
                var nr1 = actor.Tell(new NonReentrantMessage {Id = 1, Delay = TimeSpan.FromMilliseconds(500)});
                await Task.Delay(50);

                var nr2 = actor.Tell(new NonReentrantMessage {Id = 2, Delay = TimeSpan.FromMilliseconds(500)});
                await Task.Delay(50);

                var r1 = actor.Ask(new ReentrantMessage {Id = 1, Delay = TimeSpan.FromMilliseconds(100)});
                await Task.Delay(50);

                var r2 = actor.Ask(new ReentrantMessage {Id = 2, Delay = TimeSpan.FromMilliseconds(100)});
                await Task.Delay(50);

                var state = await r1;
                Assert.That(state.ReentrantInProgress, Has.Count.EqualTo(1), "Should have single message");
                Assert.That(state.ReentrantInProgress[0], Is.EqualTo(2), "Should be the second message");
                Assert.That(state.NonReentrantInProgress, Has.Count.EqualTo(1), "Should have single message");
                Assert.That(state.NonReentrantInProgress[0], Is.EqualTo(1), "Should be the first message");

                state = await r2;
                Assert.That(state.ReentrantInProgress, Has.Count.EqualTo(0), "Should not have previous message");
                Assert.That(state.NonReentrantInProgress, Has.Count.EqualTo(1), "Should still have single message");
                Assert.That(state.NonReentrantInProgress[0], Is.EqualTo(1), "Should still be the first message");

                Assert.DoesNotThrow(async () => await nr1);
                Assert.DoesNotThrow(async () => await nr2);
            }

            [Test]
            public async Task When_actor_received_reentrant_message_via_Stream()
            {
                var actor = system.FreshActorOf<TestReentrantStreamConsumerActor>();
                await actor.Tell(new Activate());

                var stream1 = system.StreamOf("sms", "s1");
                var stream2 = system.StreamOf("sms", "s2");

                var i1 = stream2.Push("1");
                await Task.Delay(10);

                var i2 = stream1.Push(2);
                await Task.Delay(10);

                var inProgress = await actor.Ask(new GetStreamMessagesInProgress());
                Assert.That(inProgress, Has.Count.EqualTo(2));
                Assert.That(inProgress[0], Is.EqualTo("1"));
                Assert.That(inProgress[1], Is.EqualTo(2));

                await i1;
                await i2;
            }

            [Test]
            public async Task When_actor_received_non_reentrant_message_via_Stream()
            {
                var actor = system.FreshActorOf<TestReentrantStreamConsumerActor>();
                await actor.Tell(new Activate());

                var stream1 = system.StreamOf("sms", "s1");
                var stream2 = system.StreamOf("sms", "s2");

                var i1 = stream1.Push(1);
                await Task.Delay(10);

                var i2 = stream2.Push("2");
                await Task.Delay(10);

                var inProgress = await actor.Ask(new GetStreamMessagesInProgress());
                Assert.That(inProgress, Has.Count.EqualTo(1));
                Assert.That(inProgress[0], Is.EqualTo(1));

                await i1;
                await i2;
            }
        }
    }
}