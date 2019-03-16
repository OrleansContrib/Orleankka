using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features.Actor_behaviors
{
    namespace Reusing_handlers_via_traits
    {
        using Behaviors;

        [TestFixture]
        class Tests
        {
            class X {}
            class Y {}

            List<string> events;

            void AssertEvents(params string[] expected) => 
                CollectionAssert.AreEqual(expected, events);

            [SetUp]
            public void SetUp() => 
                events = new List<string>();

            [Test]
            public async Task When_trait_handles_message()
            {
                Receive @base = message =>
                {
                    events.Add("base");
                    return TaskResult.Unhandled;
                };

                Task<object> XTrait(object message)
                {
                    events.Add("x");
                    return TaskResult.Unhandled;
                }

                Task<object> YTrait(object message)
                {
                    events.Add("y");
                    return TaskResult.From("y");
                }

                var receive = @base.Join(XTrait, YTrait);
                var result = await receive("foo");

                AssertEqual(new[] {"base", "x", "y"}, events);
                Assert.AreEqual("y", result);
            }

            [Test]
            public async Task When_base_handles_message()
            {
                Receive @base = message =>
                {
                    events.Add("base");
                    return TaskResult.From("base");
                };

                Task<object> XTrait(object message)
                {
                    events.Add("x");
                    return TaskResult.From("x");
                }

                Task<object> YTrait(object message)
                {
                    events.Add("y");
                    return TaskResult.From("y");
                }

                var receive = @base.Join(XTrait, YTrait);
                var result = await receive("foo");

                AssertEqual(new[] {"base"}, events);
                Assert.AreEqual("base", result);
            }

            [Test]
            public async Task When_none_of_receives_handles_message()
            {
                Receive @base = message =>
                {
                    events.Add("base");
                    return TaskResult.Unhandled;
                };

                Task<object> XTrait(object message)
                {
                    events.Add("x");
                    return TaskResult.Unhandled;
                }

                Task<object> YTrait(object message)
                {
                    events.Add("y");
                    return TaskResult.Unhandled;
                }

                var receive = @base.Join(XTrait, YTrait);
                var result = await receive("foo");

                AssertEqual(new[] {"base", "x", "y"}, events);
                Assert.AreSame(Unhandled.Result, result);
            }

            [Test]
            public async Task When_handling_lifecycle_events()
            {
                Receive @base = message =>
                {
                    events.Add("base");
                    return TaskResult.Done;
                };

                Task<object> XTrait(object message)
                {
                    events.Add("x");
                    return TaskResult.Unhandled;
                }

                Task<object> YTrait(object message)
                {
                    events.Add("y");
                    return TaskResult.Unhandled;
                }

                var receive = @base.Join(XTrait, YTrait);
                Assert.AreSame(Done.Result, await receive(Activate.Message));
                Assert.AreSame(Done.Result, await receive(Deactivate.Message));
                Assert.AreSame(Done.Result, await receive(Become.Message));
                Assert.AreSame(Done.Result, await receive(Unbecome.Message));

                // should not pass lifecycle events to traits
                AssertEqual(new[] {"base", "base", "base", "base"}, events);
            }

            [Test]
            public async Task When_composed_via_state()
            {
                Task<object> Base(object message)
                {
                    events.Add("base");
                    return TaskResult.Unhandled;
                }

                Task<object> XTrait(object message)
                {
                    events.Add("x");
                    return TaskResult.Unhandled;
                }
                
                Task<object> YTrait(object message)
                {
                    events.Add("y");
                    return TaskResult.From("y");
                }

                Receive[] Cast(params Receive[] args) => args;

                Behavior behavior = new BehaviorTester(events)
                    .State(Base, trait: Cast(XTrait, YTrait))
                    .Initial(Base);

                var result = await behavior.Receive("foo");

                AssertEqual(new[] {"base", "x", "y"}, events);
                Assert.AreEqual("y", result);
            }

            static void AssertEqual(IEnumerable<string> expected, IEnumerable<string> actual) =>
                CollectionAssert.AreEqual(expected, actual);
        }
    }
}