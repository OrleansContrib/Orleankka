using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features.Actor_behaviors
{   
    using Behaviors;

    namespace Switchable_behaviors
    {
        [TestFixture]
        class Tests
        {
            class X {}
            class Y {}
            class Z {}

            class TestActor : DispatchActorGrain
            {
                public readonly Behavior behavior;

                public TestActor(IActorRuntime runtime) : base(runtime)
                {
                    behavior = new Behavior(this)
                    {
                        OnTransitioning = transition =>
                        {
                            Events.Add($"OnTransitioning_{transition.From}_{transition.To}");
                            return Task.CompletedTask;
                        },
                        OnTransitioned = transition =>
                        {
                            Events.Add($"OnTransitioned_{transition.To}_{transition.From}");
                            return Task.CompletedTask;
                        }
                    };
                }

                public override Task<object> Receive(object message) => behavior.OnReceive(message);

                public readonly List<string> Events = new List<string>();

                void RecordTransitions(string behavior, object message)
                {
                    switch (message)
                    {
                        case Become _ :
                            Events.Add($"OnBecome_{behavior}");
                            break;
                        case Unbecome _ :
                            Events.Add($"OnUnbecome_{behavior}");
                            break;                        
                        case Activate _ :
                            Events.Add($"OnActivate_{behavior}");
                            break;                        
                        case Deactivate _ :
                            Events.Add($"OnDeactivate_{behavior}");
                            break;
                    }
                }

                public Task<object> Initial(object message)
                {
                    RecordTransitions(nameof(Initial), message);

                    return Result(Unhandled);
                }

                public async Task<object> A(object message)
                {
                    RecordTransitions(nameof(A), message);
                    
                    switch (message)
                    {
                        case X _:
                            await behavior.Become(B);
                            return Done;
                        case Reminder reminder:
                            Events.Add($"OnReminder_{reminder.Name}");
                            return Done;
                        default:
                            return Unhandled; 
                    }
                }

                public async Task<object> B(object message)
                {
                    RecordTransitions(nameof(B), message);

                    switch (message)
                    {
                        case Y _:
                            await behavior.Become(A);
                            return Done;
                        default:
                            return Unhandled;
                    }
                }
            }

            TestActor actor;

            [SetUp]
            public void SetUp()
            {
                actor = new TestActor(new MockRuntime());
            }

            [Test]
            public void When_not_specified() =>
                Assert.That(actor.behavior.CurrentName, Is.Null);

            [Test]
            public void When_setting_initial_more_than_once()
            {
                actor.behavior.Initial(actor.Initial);
                Assert.Throws<InvalidOperationException>(() => actor.behavior.Initial(actor.Initial));
            }

            [Test]
            public void When_trying_to_become_other_without_setting_initial_first() =>
                Assert.ThrowsAsync<InvalidOperationException>(async () => await actor.behavior.Become(actor.A));

            [Test]
            public void When_setting_initial()
            {
                actor.behavior.Initial(actor.Initial);

                Assert.That(actor.behavior.CurrentName, Is.EqualTo(nameof(actor.Initial)));
                Assert.That(actor.Events, Has.Count.EqualTo(0),
                    "OnBecome should not be called when setting initial");
            }

            [Test]
            public async Task When_transitioning()
            {
                actor.behavior.Initial(actor.Initial);

                await actor.behavior.Become(actor.A);
                Assert.That(actor.behavior.CurrentName, Is.EqualTo(nameof(actor.A)));

                var expected = new[]
                {
                    "OnTransitioning_Initial_A",
                    "OnDeactivate_Initial",
                    "OnUnbecome_Initial",
                    "OnBecome_A",
                    "OnActivate_A",
                    "OnTransitioned_A_Initial"
                };

                AssertEqual(expected, actor.Events);
            }

            [Test]
            public async Task When_receiving_message()
            {
                actor.behavior.Initial(actor.A);

                await actor.Receive(new X());

                Assert.That(actor.behavior.CurrentName, Is.EqualTo(nameof(actor.B)));
            }

            static void AssertEqual(IEnumerable<string> expected, IEnumerable<string> actual) => 
                CollectionAssert.AreEqual(expected, actual);
        }
    }
}