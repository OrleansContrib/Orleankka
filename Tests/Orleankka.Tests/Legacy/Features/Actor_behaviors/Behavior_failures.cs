using System;
using System.Threading.Tasks;

using NUnit.Framework;

using Orleans;

namespace Orleankka.Legacy.Features.Actor_behaviors
{
    using Behaviors;

    namespace Behavior_failures
    {
        public interface ITestActor : IActorGrain, IGrainWithStringKey
        { }

        public class TestActor : Actor, ITestActor
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

            public bool ThrowInOnTransitioning;

            public override Task OnTransitioning(Transition transition)
            {
                if (ThrowInOnTransitioning)
                    throw new ApplicationException(nameof(ThrowInOnTransitioning));

                return base.OnTransitioning(transition);
            }

            public bool ThrowInOnTransitioned;

            public override Task OnTransitioned(Transition transition)
            {
                if (ThrowInOnTransitioned)
                    throw new ApplicationException(nameof(OnTransitioned));

                return base.OnTransitioned(transition);
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

        [TestFixture]
        public class Tests
        { 
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
            public void When_faulty_OnTransitioning()
            {
                actor.ThrowInOnTransitioning = true;

                const string from = nameof(TestActor.Foo);
                const string to = nameof(TestActor.Bar);

                actor.Behavior.Initial(from);

                var exception = Assert.ThrowsAsync<ApplicationException>(async () => await actor.Become(to));
                Assert.That(exception.Message, Is.EqualTo(nameof(actor.ThrowInOnTransitioning)));

                Assert.That(actor.PassedTransition.From, Is.EqualTo(from));
                Assert.That(actor.PassedTransition.To, Is.EqualTo(to));
                Assert.That(actor.PassedException, Is.Not.Null);
                Assert.That(actor.PassedException.Message, Is.EqualTo(nameof(actor.ThrowInOnTransitioning)));
            }

            [Test]
            public void When_faulty_OnTransitioned()
            {
                actor.ThrowInOnTransitioned = true;

                const string from = nameof(TestActor.Foo);
                const string to = nameof(TestActor.Bar);

                actor.Behavior.Initial(from);

                var exception = Assert.ThrowsAsync<ApplicationException>(async () => await actor.Become(to));
                Assert.That(exception.Message, Is.EqualTo(nameof(actor.OnTransitioned)));

                Assert.That(actor.PassedTransition.From, Is.EqualTo(from));
                Assert.That(actor.PassedTransition.To, Is.EqualTo(to));
                Assert.That(actor.PassedException, Is.Not.Null);
                Assert.That(actor.PassedException.Message, Is.EqualTo(nameof(actor.OnTransitioned)));
            }

            void AssertFailure(string from, string to, string faulty)
            {
                actor.Behavior.Initial(from);

                var exception = Assert.ThrowsAsync<ApplicationException>(async () => await actor.Become(to));
                Assert.That(exception.Message, Is.EqualTo(faulty));

                Assert.That(actor.PassedTransition.From, Is.EqualTo(from));
                Assert.That(actor.PassedTransition.To, Is.EqualTo(to));
                Assert.That(actor.PassedException, Is.Not.Null);
                Assert.That(actor.PassedException.Message, Is.EqualTo(faulty));

                Assert.That(activation.DeactivateOnIdleWasCalled, Is.True);
            }
        }
    }
}