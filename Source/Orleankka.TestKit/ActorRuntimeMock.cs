using Orleankka.Services;

namespace Orleankka.TestKit
{
    public class ActorRuntimeMock : IActorRuntime
    {
        public ActorRuntimeMock(MessageSerialization serialization = null)
        {
            System      = new ActorSystemMock(serialization);
            Timers      = new TimerServiceMock();
            Jobs        = new BackgroundJobServiceMock();
            Reminders   = new ReminderServiceMock();
            Activation  = new ActivationServiceMock();
        }

        public readonly ActorSystemMock System;
        public readonly TimerServiceMock Timers;
        public readonly BackgroundJobServiceMock Jobs;
        public readonly ReminderServiceMock Reminders;
        public readonly ActivationServiceMock Activation;

        IActorSystem IActorRuntime.System => System;
        ITimerService IActorRuntime.Timers => Timers;
        IBackgroundJobService IActorRuntime.Jobs => Jobs;
        IReminderService IActorRuntime.Reminders => Reminders;
        IActivationService IActorRuntime.Activation => Activation;
    }
}
