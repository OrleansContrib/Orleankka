using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;

namespace Orleankka.Features.Actor_behaviors
{   
    using Meta;
    using Behaviors;
    using Testing;

    namespace Firing_messages
    {
        [TestFixture]
        [RequiresSilo]
        public class Tests
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

                [Behavior]
                void Test()
                {
                    this.OnReceive<GetReceived, List<string>>(x => received);
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

            [Serializable] class CheckUnhandledFiring : Command {}
            [Serializable] class GetOrigins : Query<List<Tuple<string, bool>>> { }

            class TestUnhandledFireActor : Actor
            {
                readonly List<Tuple<string, bool>> origins = new List<Tuple<string, bool>>();

                public TestUnhandledFireActor()
                {
                    Behavior.Initial(Foreground);

                    Behavior.OnUnhandledReceive((message, state, origin) =>
                    {
                        origins.Add(new Tuple<string, bool>(origin.Behavior, origin.IsBackground));
                    });
                }

                [Behavior]
                public void Foreground()
                {
                    this.OnReceive<CheckUnhandledFiring>(async x =>
                    {
                        await this.Fire("foo");
                        await this.Become(nameof(Background));
                    });
                }

                [Behavior]
                public void Background()
                {
                    this.OnActivate(()=>
                    {
                        Timers.Register("test", TimeSpan.FromMilliseconds(1), async ()=>
                        {
                            await this.Fire("bar");
                        });
                    });
                }

                public override Task<object> OnReceive(object message)
                {
                    if (message is GetOrigins)
                        return Task.FromResult((object)origins);

                    return base.OnReceive(message);
                }
            }
            [TestFixtureSetUp]
            public void FixtureSetUp()
            {
                ActorBehavior.Register(typeof(TestInterleaveFireActor));
                ActorBehavior.Register(typeof(TestUnhandledFireActor));
            }

            [Test]
            public async Task When_receive_interleaves_with_timer_ticks()
            {
                var actor = TestActorSystem.Instance.ActorOf<TestInterleaveFireActor>("test");

                await actor.Tell(new CheckInterleaveFire
                {
                    ReceiveDelay = TimeSpan.FromMilliseconds(200),
                    TimerDelay = TimeSpan.FromMilliseconds(100)
                });

                await Task.Delay(TimeSpan.FromMilliseconds(400));

                CollectionAssert.AreEqual(new[] { "timer", "timer" },
                    await actor.Ask(new GetReceived()));

                CollectionAssert.AreEqual(new[] { "main", "timer", "main", "timer" },
                    await actor.Ask(new GetFired()));
            }

            [Test]
            public async Task When_unhandled_callback_has_proper_request_origin()
            {
                var actor = TestActorSystem.Instance.ActorOf<TestUnhandledFireActor>("test");

                await actor.Tell(new CheckUnhandledFiring());
                await Task.Delay(TimeSpan.FromMilliseconds(200));

                var firstFire = new Tuple<string, bool>(nameof(TestUnhandledFireActor.Foreground), false);
                var secondFire = new Tuple<string, bool>(nameof(TestUnhandledFireActor.Background), true);

                CollectionAssert.AreEqual(new[] { firstFire, secondFire },
                    await actor.Ask(new GetOrigins()));
            }
        }
    }
}