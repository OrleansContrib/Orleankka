using System;

namespace Orleankka.Features.Actor_behaviors
{
    using Services;

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