using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features.Actor_behaviors
{
    using Behaviors;

    namespace Hierarchical_state_machine
    {
        using System;

        [TestFixture]
        class Tests
        {
            List<string> events;

            void AssertEvents(params string[] expected) => 
                CollectionAssert.AreEqual(expected, events);

            [SetUp]
            public void SetUp() => 
                events = new List<string>();

            [Test]
            public async Task When_transitioning()
            {                               
                Behavior behavior = new BehaviorTester(events)
                    .State("Initial")
                    .State("A",  super: "S")
                    .State("B",  super: "SS")
                    .State("S",  super: "SS")
                    .State("SS", super: "SSS")
                    .State("SSS")
                    .Initial("Initial");
                
                await behavior.Become("A");
                Assert.That(behavior.Current.Name, Is.EqualTo("A"));

                AssertEvents(                
                    "OnDeactivate_Initial",
                    "OnUnbecome_Initial",
                    "OnBecome_SSS",
                    "OnBecome_SS",
                    "OnBecome_S",
                    "OnBecome_A",
                    "OnActivate_SSS",
                    "OnActivate_SS",
                    "OnActivate_S",
                    "OnActivate_A"
                );
            }

            [Test]
            public async Task When_transitioning_within_same_super()
            {
                Behavior behavior = new BehaviorTester(events)
                    .State("Initial")
                    .State("A",  super: "S")
                    .State("B",  super: "SS")
                    .State("S",  super: "SS")
                    .State("SS")
                    .Initial("A");
                
                await behavior.Become("B");
                Assert.That(behavior.Current.Name, Is.EqualTo("B"));

                AssertEvents(
                    "OnDeactivate_A",
                    "OnDeactivate_S",
                    "OnUnbecome_A",
                    "OnUnbecome_S",
                    "OnBecome_B",                    
                    "OnActivate_B"
                );
            }

            [Test]
            public async Task When_transitioning_different_super()
            {
                Behavior behavior = new BehaviorTester(events)
                    .State("Initial")
                    .State("A",  super: "S")
                    .State("B",  super: "SS")
                    .State("S",  super: "SS")
                    .State("SS", super: "SSS")
                    .State("C",  super: "SSSS")
                    .State("SSS")
                    .State("SSSS")
                    .Initial("A");

                await behavior.Become("C");
                Assert.That(behavior.Current.Name, Is.EqualTo("C"));

                AssertEvents(
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
                    "OnActivate_SSSS",
                    "OnActivate_C"
                );
            }

            [Test]
            public void When_cyclic_super()
            {
                var sm = new StateMachine()
                    .State("A",  _ => null, super: "S")
                    .State("S",  _ => null, super: "SS")
                    .State("SS", _ => null, super: "A");

                var ex = Assert.Throws<InvalidOperationException>(() => sm.Build());
                Assert.That(ex.Message, Is.EqualTo("Cycle detected: A -> S -> SS !-> A"));
            }

            [Test]
            public async Task When_receiving_activate_message_initial()
            {
                var behavior = new Behavior(new StateMachine()
                    .State("A", _ => TaskResult.Done)
                    .State("B", _ => TaskResult.Done));

                behavior.Initial("A");

                var result = await behavior.Receive(Activate.Message);
                Assert.That(result, Is.SameAs(Done.Result));
            }

            [Test]
            public async Task When_receiving_deactivate_message_initial()
            {
                var behavior = new Behavior(new StateMachine()
                    .State("A", _ => TaskResult.Done)
                    .State("B", _ => TaskResult.Done));

                behavior.Initial("A");

                var result = await behavior.Receive(Deactivate.Message);
                Assert.That(result, Is.SameAs(Done.Result));
            }

            [Test]
            public async Task When_receiving_message_should_check_handlers_in_a_chain()
            {
                Task<object> AReceive(object message) => message is string
                    ? Task.FromResult<object>("foo") 
                    : TaskResult.Unhandled;

                Task<object> SReceive(object message) => message is int
                    ? Task.FromResult<object>("bar") 
                    : TaskResult.Unhandled;

                var behavior = new Behavior(new StateMachine()
                    .State("A", AReceive, super: "S")
                    .State("S", SReceive));

                behavior.Initial("A");

                Assert.That(await behavior.Receive("1"), Is.EqualTo("foo"));
                Assert.That(await behavior.Receive(1), Is.EqualTo("bar"));
                Assert.That(await behavior.Receive(DateTime.Now), Is.SameAs(ActorGrain.Unhandled));
            }
        }
    }
}