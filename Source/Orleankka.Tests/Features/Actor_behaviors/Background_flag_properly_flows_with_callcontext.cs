using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;

namespace Orleankka.Features.Actor_behaviors
{   
    using Meta;
    using Behaviors;
    using Testing;
    using Utility;

    namespace Background_flag_properly_flows_with_callcontext
    {
        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            [Serializable] class Activate  {}
            [Serializable] class Copied  {}
            [Serializable] class Failed  {}
            [Serializable] class Suspend  {}
            [Serializable] class GetEvents : Query<List<string>> {}

            class TestActor : Actor
            {
                readonly List<string> events = new List<string>();

                public TestActor()
                {
                    Behavior.Initial(Copying);
                }

                public override Task OnTransitioned(string current, string previous)
                {
                    events.Add($"{previous}->{current}");
                    return Task.CompletedTask;
                }

                public override Task<object> OnUnhandledReceive(RequestOrigin origin, object message)
                {
                    if (origin.IsBackground && Behavior.Current != origin.Behavior)
                    {
                        events.Add($"Received {message.GetType().Name} from background {origin.Behavior} while current is {Behavior.Current}");
                        return TaskResult.Done;
                    }

                    events.Add($"Unhandled {message.GetType().Name} from {origin.Behavior}[Background={origin.IsBackground}] while current is {Behavior.Current}");
                    throw new UnhandledMessageException(this, message);                    
                }

                public override async Task<object> OnReceive(object message)
                {
                    if (message is Activate)
                        return null;

                    if (message is GetEvents)
                        return events;

                    return await base.OnReceive(message);
                }

                [Behavior] void Active()
                {
                    this.OnActivate(() => events.Add("Become Active"));
                }

                [Trait] void Cancellable()
                {
                    this.OnReceive<Suspend>(async x =>
                    {
                        events.Add("Received Suspend");
                        await this.Become(Suspended);
                    });
                }

                [Behavior] void Copying()
                {
                    this.Super(Active);
                    this.Trait(Cancellable);

                    this.OnActivate(() => Timers.Register("copy", TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(1), () => Try(Copy)));

                    this.OnDeactivate(async () =>
                    {
                        await Task.Delay(100); // give time for timer to fire
                        Timers.Unregister("copy");
                    });

                    this.OnReceive<Copied>(async x =>
                    {
                        events.Add("Received Copied");
                        await Task.Delay(100);
                    });
                }

                // call via intermediary methods
                async Task Try(Func<Task> action)
                {
                    try
                    {
                        await action();
                    }
                    catch (Exception)
                    {
                        await this.Fire(new Failed());
                    }
                }

                async Task Copy()
                {
                    await Task.Delay(10);
                    await this.Fire(new Copied());
                }

                [Behavior] void Suspended()
                {
                    this.Super(Active);

                    this.OnActivate(() => events.Add("Become Suspended"));
                }
            }

            [TestFixtureSetUp]
            public void FixtureSetUp()
            {
                ActorBehavior.Register(typeof(TestActor));
            }

            [Test]
            public async Task When_receive_interleaves_with_timer_ticks()
            {
                var actor = TestActorSystem.Instance.ActorOf<TestActor>("test");

                await actor.Tell(new Activate());
                await Task.Delay(TimeSpan.FromMilliseconds(150));

                // switch behavior externally
                await actor.Tell(new Suspend());
                await Task.Delay(TimeSpan.FromMilliseconds(100));

                var events = await actor.Ask<List<string>>(new GetEvents());
                var expected = new[]
                {
                    "Become Active",
                    "Received Copied",
                    "Received Suspend",
                    "Copying->Suspended",
                    "Become Suspended",
                    "Received Copied from background Copying while current is Suspended" // this arrives after the behavior is switched
                };

                CollectionAssert.AreEqual(expected, events);
            }
        }
    }
}