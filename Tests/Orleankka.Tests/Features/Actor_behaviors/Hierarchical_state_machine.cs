using System.Collections.Generic;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features.Actor_behaviors
{
    using Behaviors;

    namespace Hierarchical_state_machine
    {
        [TestFixture]
        class Tests
        {
            class X {}
            class Y {}
            class Z {}

            public interface ITestActor : IActorGrain
            { }

            public class TestActor : ActorGrain, ITestActor
            {
                public Behavior Behavior { get; private set; }

                public void Setup(StateMachine sm)
                {
                    Behavior = new Behavior(sm, OnTransitioning, OnTransitioned);
                }

                Task OnTransitioning(Transition transition)
                {
                    Events.Add($"OnTransitioning_{transition.From}_{transition.To}");
                    return Task.CompletedTask;
                }

                Task OnTransitioned(Transition transition)
                {
                    Events.Add($"OnTransitioned_{transition.To}_{transition.From}");
                    return Task.CompletedTask;
                }

                public override Task<object> Receive(object message) => Behavior.Receive(message);

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
                            await Behavior.Become(B);
                            break;
                    }

                    return Done;
                }

                public async Task<object> B(object message)
                {
                    RecordTransitions(nameof(B), message);

                    switch (message)
                    {
                        case Y _:
                            await Behavior.Become(A);
                            break;
                    }

                    return Done;
                }
                
                public Task<object> C(object message)
                {
                    RecordTransitions(nameof(C), message);
                    return Result(Unhandled);
                }

                public Task<object> S(object message)
                {
                    RecordTransitions(nameof(S), message);
                    return Result(Unhandled);
                }

                public async Task<object> SS(object message)
                {
                    RecordTransitions(nameof(SS), message);

                    switch (message)
                    {
                        case Z _:
                            await Behavior.Become(C);
                            break;
                    }

                    return Done;
                }

                public Task<object> SSS(object message)
                {
                    RecordTransitions(nameof(SSS), message);
                    return Result(Unhandled);
                }
                
                public Task<object> SSSS(object message)
                {
                    RecordTransitions(nameof(SSSS), message);
                    return Result(Unhandled);
                }
            }

            TestActor actor;

            [SetUp]
            public void SetUp()
            {
                actor = new TestActor();
                
                var sm = new StateMachine()
                    .State(actor.A,  super: actor.S)
                    .State(actor.C,  super: actor.SSSS)
                    .State(actor.S,  super: actor.SS)
                    .State(actor.B,  super: actor.SS)
                    .State(actor.SS, super: actor.SSS)
                    .State(actor.SSS)
                    .State(actor.SSSS);

                actor.Setup(sm);
            }

            [Test]
            public async Task When_transitioning()
            {
                actor.Behavior.Initial(actor.Initial);
                
                await actor.Behavior.Become(actor.A);

                Assert.That(actor.Behavior.Current, Is.EqualTo(nameof(actor.A)));
                
                var expected = new[]
                {
                    "OnTransitioning_Initial_A",
                    "OnDeactivate_Initial",
                    "OnUnbecome_Initial",
                    "OnBecome_SSS",
                    "OnBecome_SS",
                    "OnBecome_S",
                    "OnBecome_A",
                    "OnActivate_SSS",
                    "OnActivate_SS",
                    "OnActivate_S",
                    "OnActivate_A",
                    "OnTransitioned_A_Initial"
                };

                AssertEqual(expected, actor.Events);
            }

            [Test]
            public async Task When_transitioning_within_same_super()
            {
                actor.Behavior.Initial(actor.Initial);
                
                await actor.Behavior.Become(actor.A);
                actor.Events.Clear();

                await actor.Behavior.Become(actor.B);
                var expected = new[]
                {
                    "OnTransitioning_A_B",
                    "OnDeactivate_A",
                    "OnDeactivate_S",
                    "OnUnbecome_A",
                    "OnUnbecome_S",
                    "OnBecome_B",                    
                    "OnActivate_B",
                    "OnTransitioned_B_A"
                };

                AssertEqual(expected, actor.Events);
            }

            static void AssertEqual(IEnumerable<string> expected, IEnumerable<string> actual) => 
                CollectionAssert.AreEqual(expected, actual);
        }
    }
}