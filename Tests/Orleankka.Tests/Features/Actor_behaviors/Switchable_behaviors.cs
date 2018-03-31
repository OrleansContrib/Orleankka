using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            List<string> events;

            void AssertEvents(params string[] expected) => 
                CollectionAssert.AreEqual(expected, events);

            [SetUp]
            public void SetUp() => 
                events = new List<string>();

            [Test]
            public void When_not_specified()
            {
                var behavior = new Behavior();
                Assert.That(behavior.Current, Is.Null);
            }

            [Test]
            public void When_setting_initial_more_than_once()
            {
                var behavior = new Behavior();
                behavior.Initial(message => TaskResult.Done);
                Assert.Throws<InvalidOperationException>(() => behavior.Initial(message => TaskResult.Done));
            }

            [Test]
            public void When_trying_to_become_other_without_setting_initial_first()
            {
                var behavior = new Behavior();
                Assert.ThrowsAsync<InvalidOperationException>(async () => await behavior.Become(x => TaskResult.Done));
            }

            [Test]
            public void When_setting_initial()
            {
                Behavior behavior = new BehaviorTester(events)
                    .State("Initial");

                behavior.Initial("Initial");

                Assert.That(behavior.Current, Is.EqualTo("Initial"));
                Assert.That(events, Has.Count.EqualTo(0),
                    "OnBecome should not be called when setting initial");
            }
            
            [Test]
            public async Task When_transitioning()
            {
                Behavior behavior = new BehaviorTester(events)
                    .State("Initial")
                    .State("A");

                behavior.Initial("Initial");

                await behavior.Become("A");
                Assert.That(behavior.Current, Is.EqualTo("A"));

                var expected = new[]
                {
                    "OnTransitioning_Initial_A",
                    "OnDeactivate_Initial",
                    "OnUnbecome_Initial",
                    "OnBecome_A",
                    "OnActivate_A",
                    "OnTransitioned_A_Initial"
                };

                AssertEvents(expected);
            }

            [Test]
            public void When_returns_null_task()
            {
                Behavior behavior = new BehaviorTester(events)
                    .State("A", x => null)
                    .Initial("A");

                var exception = Assert.ThrowsAsync<InvalidOperationException>(async ()=> await behavior.Receive("foo"));
                Assert.That(exception.Message, Is.EqualTo("Behavior returns null task on handling 'foo' message"));
            }

            [Test]
            public async Task When_receiving_message()
            {
                Task<object> Receive(object x) => x is string 
                    ? Task.FromResult<object>("foo") 
                    : Task.FromResult<object>("bar");

                Behavior behavior = new BehaviorTester(events)
                    .State("A", Receive);

                behavior.Initial("A");

                Assert.That(await behavior.Receive("1"), Is.EqualTo("foo"));
                Assert.That(await behavior.Receive(1), Is.EqualTo("bar"));
            }

            [Test]
            [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
            public void When_becoming_other_during_transition()
            {
                async Task<object> AttemptBecomeDuring<T>(Behavior b, string other, object message)
                {
                    if (message is T)
                        await b.Become(other);

                    return null;
                }

                Behavior behavior = null;

                behavior = new BehaviorTester(events)
                    .State("A", x => AttemptBecomeDuring<Deactivate>(behavior, "C", x))
                    .State("B")
                    .State("C")
                    .Initial("A");

                Assert.ThrowsAsync<InvalidOperationException>(async () => await behavior.Become("B"));

                behavior = new BehaviorTester(events)
                    .State("A", x => AttemptBecomeDuring<Unbecome>(behavior, "C", x))
                    .State("B")
                    .State("C")
                    .Initial("A");
               
                Assert.ThrowsAsync<InvalidOperationException>(async () => await behavior.Become("B"));

                behavior = new BehaviorTester(events)
                    .State("A")
                    .State("B", x => AttemptBecomeDuring<Activate>(behavior, "C", x))
                    .State("C")
                    .Initial("A");
               
                Assert.ThrowsAsync<InvalidOperationException>(async () => await behavior.Become("B"));

                behavior = new BehaviorTester(events)
                    .State("A")
                    .State("B", x => AttemptBecomeDuring<Become>(behavior, "C", x))
                    .State("C")
                    .Initial("A");
               
                Assert.ThrowsAsync<InvalidOperationException>(async () => await behavior.Become("B"));
            }
        }
    }
}