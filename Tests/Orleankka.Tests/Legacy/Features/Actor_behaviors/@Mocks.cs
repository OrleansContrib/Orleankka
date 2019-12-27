using System;

using Orleankka.Services;

namespace Orleankka.Legacy.Features.Actor_behaviors
{
    class MockRuntime : IActorRuntime
    {
        public readonly MockActorSystem MockActorSystem = new MockActorSystem();
        public readonly MockActivationService MockActivationService = new MockActivationService();

        public IActorSystem System => MockActorSystem;
        public ITimerService Timers => throw new NotImplementedException();
        public IBackgroundJobService Jobs => throw new NotImplementedException();
        public IReminderService Reminders => throw new NotImplementedException();
        public IActivationService Activation => MockActivationService;
    }

    class MockActorSystem : IActorSystem
    {
        public ActorRef ActorOf(ActorPath path) => throw new NotImplementedException();
        public StreamRef<TItem> StreamOf<TItem>(StreamPath path) => throw new NotImplementedException();
        public ClientRef ClientOf(string path) => throw new NotImplementedException();
    }

    class MockActivationService : IActivationService
    {
        public bool DeactivateOnIdleWasCalled;
        public void DeactivateOnIdle() => DeactivateOnIdleWasCalled = true;
        public void DelayDeactivation(TimeSpan period) => throw new NotImplementedException();
    }
}