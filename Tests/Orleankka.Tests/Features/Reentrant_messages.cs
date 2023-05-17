using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka.Features
{
    namespace Reentrant_messages
    {
        using Meta;
        using Orleans.Metadata;
        using Orleans.Serialization.Invocation;

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
        class ActorState : Result
        {
            public readonly List<int> ReentrantInProgress = new List<int>();
            public readonly List<int> NonReentrantInProgress = new List<int>();
        }

        [DefaultGrainType("reentrant-test")]
        public interface ITestActor : IActorGrain, IGrainWithStringKey
        {}

        [MayInterleave(nameof(Interleave))]
        [GrainType("reentrant-test")]
        public  class TestActor : DispatchActorGrain, ITestActor
        {
            public static bool Interleave(IInvokable req) => req.Message() is ReentrantMessage;

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

        public interface ITestReentrantByCallbackMethodActor : IActorGrain, IGrainWithStringKey
        {}

        [MayInterleave(nameof(Interleave))]
        public class TestReentrantByCallbackMethodActor : DispatchActorGrain, ITestReentrantByCallbackMethodActor
        {
            public static bool Interleave(IInvokable req) => req.Message() is ReentrantMessage;

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

        public interface ITestReentrantByCallbackMethodActorFromAnotherActor : IActorGrain, IGrainWithStringKey
        {}

        [Reentrant]
        public class TestReentrantByCallbackMethodActorFromAnotherActor : DispatchActorGrain, ITestReentrantByCallbackMethodActorFromAnotherActor
        {
            ActorRef receiver;

            public override async Task<object> Receive(object message)
            {
                switch (message)
                {
                    case Activate _: 
                        receiver = System.FreshActorOf<ITestReentrantByCallbackMethodActor>();
                        break;
                    case Message _:
                        return await receiver.Ask<object>(message);
                    default: 
                        return await base.Receive(message);
                }

                return Done;
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
                var actor = system.FreshActorOf<ITestActor>();
                await TestReentrantReceive(actor);
            }

            [Test]
            public async Task When_reentrant_determined_by_callback_method()
            {
                var actor = system.FreshActorOf<ITestReentrantByCallbackMethodActor>();
                await TestReentrantReceive(actor);
            }

            [Test]
            public async Task When_reentrant_determined_by_callback_method_sent_from_another_actor()
            {
                var actor = system.FreshActorOf<ITestReentrantByCallbackMethodActorFromAnotherActor>();
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

                Assert.DoesNotThrowAsync(async () => await nr1);
                Assert.DoesNotThrowAsync(async () => await nr2);
            }
        }
    }
}