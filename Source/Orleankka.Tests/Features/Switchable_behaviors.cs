using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
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

            class TestActor : Actor
            {
                public readonly List<string> Events = new List<string>();

                public TestActor()
                {
                    Behavior.OnBecome((current, previous) => Events.Add($"OnBecome_{current}_{previous}"));
                }

                void Setup(string behavior)
                {
                    this.OnBecome(() => Events.Add($"OnBecome_{behavior}"));
                    this.OnUnbecome(() => Events.Add($"OnUnbecome_{behavior}"));
                    this.OnActivate(() => Events.Add($"OnActivate_{behavior}"));
                    this.OnDeactivate(() => Events.Add($"OnDeactivate_{behavior}"));
                }

                [Behavior] public void Initial() => Setup(nameof(Initial));

                [Behavior] public void A()
                {
                    Setup(nameof(A));
                    this.Super(S);
                    this.OnReceive<X>(x => this.Become(B));
                    this.OnReminder(id => Events.Add($"OnReminder_{id}"));
                }

                [Behavior] public void B()
                {
                    Setup(nameof(B));
                    this.Super(SS);
                    this.OnReceive<Y>(x => this.Become(A));
                }

                [Behavior] void S()
                {
                    Setup(nameof(S));
                    this.Super(SS);
                }

                [Behavior] void SS()
                {
                    Setup(nameof(SS));
                    this.Super(SSS);
                    this.OnReceive<Z>(x => this.Become(C));
                    this.OnReminder("foo", ()=> Events.Add("OnReminder_foo"));
                }

                [Behavior] void SSS()
                {
                    Setup(nameof(SSS));
                }

                [Behavior] public void C()
                {
                    Setup(nameof(C));
                    this.Super(SSSS);
                }

                [Behavior] void SSSS()
                {
                    Setup(nameof(SSSS));
                }

                [Behavior] public void CyclicSuperA()
                {
                    Setup(nameof(CyclicSuperA));
                    this.Super(CyclicSuperS);
                }

                [Behavior] void CyclicSuperS()
                {
                    Setup(nameof(CyclicSuperS));
                    this.Super(CyclicSuperSS);
                }

                [Behavior] void CyclicSuperSS()
                {
                    Setup(nameof(CyclicSuperSS));
                    this.Super(CyclicSuperS); // cycle
                }

                [Behavior] public void BecomeOtherOnBecome() => this.OnBecome(() => this.Become(B));
                [Behavior] public void BecomeOtherOnActivate() => this.OnActivate(() => this.Become(B));

                [Behavior] public void BecomeOtherOnUnbecome() => this.OnUnbecome(() => this.Become(B));
                [Behavior] public void BecomeOtherOnDeactivate() => this.OnDeactivate(() => this.Become(B));
            }

            TestActor actor;

            [TestFixtureSetUp]
            public void FixtureSetUp()
            {
                ActorBehavior.Register(typeof(TestActor));
            }

            [SetUp]
            public void SetUp()
            {
                actor = new TestActor();
            }
            
            [Test]
            public void When_not_specified() => 
                Assert.That(actor.Behavior.Current, Is.EqualTo("<NULL>"));

            [Test]
            public void When_setting_initial_and_method_doesnt_exists() => 
                Assert.Throws<InvalidOperationException>(()=> actor.Behavior.Initial("Initial_"));

            [Test]
            public void When_setting_initial_and_method_doesnt_conform() => 
                Assert.Throws<InvalidOperationException>(()=> actor.Behavior.Initial("Setup"));

            [Test]
            public void When_setting_initial_more_than_once()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));
                Assert.Throws<InvalidOperationException>(()=> actor.Behavior.Initial(nameof(TestActor.Initial)));
            }

            [Test]
            public void When_trying_to_become_other_without_setting_initial_first() => 
                Assert.Throws<InvalidOperationException>(async ()=> await actor.Behavior.Become(actor.A));

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
                    "OnDeactivate_Initial",
                    "OnUnbecome_Initial",
                    "OnBecome_SSS",
                    "OnBecome_SS",
                    "OnBecome_S",
                    "OnBecome_A",
                    "OnBecome_A_Initial",
                    "OnActivate_SSS",
                    "OnActivate_SS",
                    "OnActivate_S",
                    "OnActivate_A"
                };

                AssertEqual(expected, actor.Events);
            }

            [Test]
            public async Task When_transitioning_within_same_super()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));

                await actor.Behavior.Become(actor.A);
                actor.Events.Clear();

                await actor.Behavior.Become(actor.B);
                var expected = new[]
                {
                    "OnDeactivate_A",
                    "OnDeactivate_S",
                    "OnUnbecome_A",
                    "OnUnbecome_S",
                    "OnBecome_B",
                    "OnBecome_B_A",
                    "OnActivate_B",
                };

                AssertEqual(expected, actor.Events);
            }

            [Test]
            public async Task When_transitioning_different_super()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));

                await actor.Behavior.Become(actor.A);
                actor.Events.Clear();

                await actor.Behavior.Become(actor.C);
                var expected = new[]
                {
                    "OnDeactivate_A",
                    "OnDeactivate_S",
                    "OnDeactivate_SS",
                    "OnDeactivate_SSS",
                    "OnUnbecome_A",
                    "OnUnbecome_S",
                    "OnUnbecome_SS",
                    "OnUnbecome_SSS",
                    "OnBecome_SSSS",
                    "OnBecome_C",
                    "OnBecome_C_A",
                    "OnActivate_SSSS",
                    "OnActivate_C"
                };

                AssertEqual(expected, actor.Events);
            }

            [Test]
            public void When_cyclic_super()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));

                Assert.Throws<InvalidOperationException>(async ()=> await 
                    actor.Behavior.Become(actor.CyclicSuperA));
            }

            [Test]
            public async Task When_receiving_message()
            {
                actor.Behavior.Initial(nameof(TestActor.A));

                await actor.OnReceive(new X());

                Assert.That(actor.Behavior.Current, Is.EqualTo(nameof(actor.B)));
            }

            [Test]
            public void When_receiving_unhandled_message()
            {
                actor.Behavior.Initial(nameof(TestActor.A));

                Assert.Throws<UnhandledMessageException>(async ()=> await actor.OnReceive(new Y()), 
                    "Should throw by default");
            }

            [Test]
            public async Task When_receiving_unhandled_message_callback()
            {
                actor.Behavior.Initial(nameof(TestActor.A));

                string passedState = null;
                object passedMessage = null;
                actor.Behavior.OnUnhandledReceive((message, state) =>
                {
                    passedState = state;
                    passedMessage = message;
                    return "test";
                });

                var msg = new Y();
                var result = await actor.OnReceive(msg);

                Assert.That(result, Is.EqualTo("test"));
                Assert.That(passedState, Is.EqualTo(nameof(TestActor.A)));
                Assert.That(passedMessage, Is.SameAs(msg));
            }

            [Test]
            public async Task When_receiving_reminder()
            {
                actor.Behavior.Initial(nameof(TestActor.A));

                await actor.OnReminder("test");

                Assert.That(actor.Events.Count, Is.EqualTo(1));
                Assert.That(actor.Events[0], Is.EqualTo("OnReminder_test"));
            }

            [Test]
            public void When_receiving_unhandled_reminder()
            {
                actor.Behavior.Initial(nameof(TestActor.B));

                Assert.Throws<UnhandledReminderException>(async ()=> await actor.OnReminder("test"));
            }

            [Test]
            public async Task When_receiving_unhandled_reminder_callback()
            {
                actor.Behavior.Initial(nameof(TestActor.B));

                string passedState = null;
                string passedReminderId = null;
                actor.Behavior.OnUnhandledReminder((reminder, state) =>
                {
                    passedState = state;
                    passedReminderId = reminder;
                });

                await actor.OnReminder("test");
                Assert.That(passedState, Is.EqualTo(nameof(TestActor.B)));
                Assert.That(passedReminderId, Is.SameAs("test"));
            }

            [Test]
            public async Task When_receiving_message_should_check_handlers_in_super_chain()
            {
                actor.Behavior.Initial(nameof(TestActor.A));

                await actor.OnReceive(new Z());

                Assert.That(actor.Behavior.Current, Is.EqualTo(nameof(actor.C)));
            }

            [Test]
            public async Task When_receiving_reminder_should_check_handlers_in_super_chain()
            {
                actor.Behavior.Initial(nameof(TestActor.B));

                await actor.OnReminder("foo");

                Assert.That(actor.Events.Count, Is.EqualTo(1));
                Assert.That(actor.Events[0], Is.EqualTo("OnReminder_foo"));
            }

            [Test]
            public async Task When_becoming_other_during_activate()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));

                await actor.Become(actor.BecomeOtherOnActivate);

                Assert.That(actor.Behavior.Current, Is.EqualTo(nameof(actor.B)));
            }

            [Test]
            public void When_becoming_other_during_become()
            {
                actor.Behavior.Initial(nameof(TestActor.Initial));

                Assert.Throws<InvalidOperationException>(async () => await actor.Become(actor.BecomeOtherOnBecome));
            }

            [Test]
            public void When_becoming_other_during_unbecome()
            {
                actor.Behavior.Initial(nameof(TestActor.BecomeOtherOnUnbecome));

                Assert.Throws<InvalidOperationException>(async ()=> await actor.Become(actor.A));
            }

            [Test]
            public void When_becoming_other_during_deactivate()
            {
                actor.Behavior.Initial(nameof(TestActor.BecomeOtherOnDeactivate));

                Assert.Throws<InvalidOperationException>(async () => await actor.Become(actor.A));
            }

            static void AssertEqual(IEnumerable<string> expected, IEnumerable<string> actual) => 
                CollectionAssert.AreEqual(expected, actual);
        }
    }
}