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

            class TestActor : ActorGrain
            {
                public TestActor(IActorRuntime runtime) : base(runtime)
                {
                    Behavior.OnTransitioning = transition =>
                    {
                        Events.Add($"OnTransitioning_{transition.From}_{transition.To}");
                        return Task.CompletedTask;
                    };

                    Behavior.OnTransitioned = transition =>
                    {
                        Events.Add($"OnTransitioned_{transition.To}_{transition.From}");
                        return Task.CompletedTask;
                    };
                }

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

                [Behavior] public Task<object> Initial(object message)
                {
                    RecordTransitions(nameof(Initial), message);

                    return Result(Unhandled);
                }

                [Behavior] public async Task<object> A(object message)
                {
                    RecordTransitions(nameof(A), message);
                    
                    switch (message)
                    {
                        case X _:
                            await this.Become(B);
                            return Done;
                        case Reminder reminder:
                            Events.Add($"OnReminder_{reminder.Name}");
                            return Done;
                        default:
                            return Unhandled; 
                    }
                }

                [Behavior] public async Task<object> B(object message)
                {
                    RecordTransitions(nameof(B), message);

                    switch (message)
                    {
                        case Y _:
                            await this.Become(A);
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
                Assert.That(actor.Behavior.Current, Is.Null);

            [Test]
            public void When_setting_initial_and_method_doesnt_exists() =>
                Assert.Throws<InvalidOperationException>(() => actor.Behavior.Initial("Initial_"));

            [Test]
            public void When_setting_initial_and_method_doesnt_conform() =>
                Assert.Throws<InvalidOperationException>(() => actor.Behavior.Initial("Setup"));

            [Test]
            public void When_setting_initial_more_than_once()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));
                Assert.Throws<InvalidOperationException>(() => actor.Behavior.Initial(nameof(TestActor.Initial)));
            }

            [Test]
            public void When_trying_to_become_other_without_setting_initial_first() =>
                Assert.ThrowsAsync<InvalidOperationException>(async () => await actor.Behavior.Become(actor.A));

            [Test]
            public void When_setting_initial()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));

                Assert.That(actor.Behavior.Current, Is.EqualTo(nameof(actor.Initial)));
                Assert.That(actor.Events, Has.Count.EqualTo(0),
                    "OnBecome should not be called when setting initial");
            }

            [Test]
            public async Task When_transitioning()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));

                await actor.Behavior.Become(actor.A);
                Assert.That(actor.Behavior.Current, Is.EqualTo(nameof(actor.A)));

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
                actor.Behavior.Initial(nameof(TestActor.A));

                await actor.Receive(new X());

                Assert.That(actor.Behavior.Current, Is.EqualTo(nameof(actor.B)));
            }

            static void AssertEqual(IEnumerable<string> expected, IEnumerable<string> actual) => 
                CollectionAssert.AreEqual(expected, actual);
        }
    }
}