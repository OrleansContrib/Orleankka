using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features.Actor_behaviors
{
    using Behaviors;
    using Services;

    namespace Behavior_failures
    {
        [TestFixture]
        public class Tests
        {
            class TestActor : Actor
            {
                TestActor()
                {}

                public TestActor(IActorRuntime runtime)
                    : base(runtime)
                {}

                public Transition PassedTransition;
                public Exception PassedException;

                public override Task OnTransitionFailure(Transition transition, Exception exception)
                {
                    PassedTransition = transition;
                    PassedException = exception;

                    return base.OnTransitionFailure(transition, exception);
                }

                public bool ThrowInOnTransitioned = false;

                public override Task OnTransitioned(string current, string previous)
                {
                    if (ThrowInOnTransitioned)
                        throw new ApplicationException(nameof(OnTransitioned));

                    return base.OnTransitioned(current, previous);
                }

                [Behavior] public void Foo() {}
                [Behavior] public void Bar() {}

                [Behavior] public void FaultyBecome() => 
                    this.OnBecome(()=> throw new ApplicationException(nameof(FaultyBecome)));

                [Behavior] public void FaultyActivate() => 
                    this.OnActivate(()=> throw new ApplicationException(nameof(FaultyActivate)));

                [Behavior] public void FaultyUnbecome() => 
                    this.OnUnbecome(()=> throw new ApplicationException(nameof(FaultyUnbecome)));

                [Behavior] public void FaultyDeactivate() => 
                    this.OnDeactivate(()=> throw new ApplicationException(nameof(FaultyDeactivate)));
            }

            TestActor actor;
            MockActivationService activation;

            [SetUp]
            public void SetUp()
            {
                var runtime = new MockRuntime();
                activation = runtime.MockActivationService;
                actor = new TestActor(runtime);
            }

            [Test]
            public void When_faulty_become()
            {
                const string from = nameof(TestActor.Foo);
                const string to = nameof(TestActor.FaultyBecome);

                AssertFailure(from, to, faulty: to);
            }

            [Test]
            public void When_faulty_activate()
            {
                const string from = nameof(TestActor.Foo);
                const string to = nameof(TestActor.FaultyActivate);

                AssertFailure(from, to, faulty: to);
            }

            [Test]
            public void When_faulty_unbecome()
            {
                const string from = nameof(TestActor.FaultyUnbecome);
                const string to = nameof(TestActor.Foo);

                AssertFailure(from, to, faulty: from);
            }

            [Test]
            public void When_faulty_deactivate()
            {
                const string from = nameof(TestActor.FaultyDeactivate);
                const string to = nameof(TestActor.Foo);

                AssertFailure(from, to, faulty: from);
            }

            [Test]
            public void When_faulty_OnTransitioned()
            {
                actor.ThrowInOnTransitioned = true;

                const string from = nameof(TestActor.Foo);
                const string to = nameof(TestActor.Bar);

                actor.Behavior.Initial(from);

                var exception = Assert.Throws<ApplicationException>(async () => await actor.Become(to));
                Assert.That(exception.Message, Is.EqualTo(nameof(actor.OnTransitioned)));

                Assert.That(actor.PassedTransition.From, Is.EqualTo(from));
                Assert.That(actor.PassedTransition.To, Is.EqualTo(to));
                Assert.That(actor.PassedException, Is.Not.Null);
                Assert.That(actor.PassedException.Message, Is.EqualTo(nameof(actor.OnTransitioned)));
            }

            void AssertFailure(string from, string to, string faulty)
            {
                actor.Behavior.Initial(from);

                var exception = Assert.Throws<ApplicationException>(async () => await actor.Become(to));
                Assert.That(exception.Message, Is.EqualTo(faulty));

                Assert.That(actor.PassedTransition.From, Is.EqualTo(from));
                Assert.That(actor.PassedTransition.To, Is.EqualTo(to));
                Assert.That(actor.PassedException, Is.Not.Null);
                Assert.That(actor.PassedException.Message, Is.EqualTo(faulty));

                Assert.That(activation.DeactivateOnIdleWasCalled, Is.True);
            }

            class MockRuntime : IActorRuntime
            {
                public readonly MockActivationService MockActivationService = new MockActivationService();

                public IActorSystem System => throw new NotImplementedException();
                public ITimerService Timers => throw new NotImplementedException();
                public IReminderService Reminders => throw new NotImplementedException();
                public IActivationService Activation => MockActivationService;
            }

            class MockActivationService : IActivationService
            {
                public bool DeactivateOnIdleWasCalled;
                public void DeactivateOnIdle() => DeactivateOnIdleWasCalled = true;
                public void DelayDeactivation(TimeSpan period) => throw new NotImplementedException();
            }
        }
    }
}