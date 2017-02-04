namespace Orleankka.Features.Actor_behaviors
{
    using NUnit.Framework;

    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    
    using Meta;
    using Behaviors;
    using Testing;

    [TestFixture]
    [RequiresSilo]
    public class Firing_messages
    {
        [Serializable]
        class CheckInterleaveFire : Command
        {
            public TimeSpan ReceiveDelay;
            public TimeSpan TimerDelay;
        }

        [Serializable] class GetReceived : Query<List<string>> {}
        [Serializable] class GetFired : Query<List<string>> {}

        [Reentrant(typeof(string))]
        class TestInterleaveFireActor : Actor
        {
            readonly List<string> received = new List<string>();
            readonly List<string> fired = new List<string>();

            public TestInterleaveFireActor()
            {
                Behavior.Initial(Test);
            }

            [Behavior] void Test()
            {
                this.OnReceive< GetReceived, List<string>>(x => received);
                this.OnReceive<GetFired, List<string>>(x => fired);

                this.OnReceive<CheckInterleaveFire>(async x =>
                {
                    await this.Fire("main");

                    Timers.Register("test", x.TimerDelay, async () =>
                    {
                        await this.Fire("timer");

                        await Task.Delay(x.TimerDelay);

                        await this.Fire("timer");
                    });

                    await Task.Delay(x.ReceiveDelay);

                    await this.Fire("main");
                });

                this.OnReceive<string>(x => fired.Add(x));
            }

            public override Task<object> OnReceive(object message)
            {
                if (message is string)
                    received.Add(message.ToString());

                return base.OnReceive(message);
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            ActorBehavior.Register(typeof(TestInterleaveFireActor));
        }

        [Test]
        public async Task When_receive_interleaves_with_timer_ticks()
        {
            var actor = TestActorSystem.Instance.ActorOf<TestInterleaveFireActor>("test");

            await actor.Tell(new CheckInterleaveFire
            {
                ReceiveDelay = TimeSpan.FromMilliseconds(100),
                TimerDelay = TimeSpan.FromMilliseconds(70)
            });

            await Task.Delay(TimeSpan.FromMilliseconds(50));

            CollectionAssert.AreEqual(new[] { "timer", "timer" }, 
                await actor.Ask(new GetReceived()));

            CollectionAssert.AreEqual(new[] { "main", "timer", "main", "timer" },
                await actor.Ask(new GetFired()));
        }
    }
}