using Orleankka.Services;

namespace Orleankka.TestKit
{
    public class ActorRuntimeMock : IActorRuntime
    {
        public ActorRuntimeMock(SerializationOptions serialization = null)
        {
            System      = new ActorSystemMock(serialization);
            Timers      = new TimerServiceMock();
            Reminders   = new ReminderServiceMock();
            Activation  = new ActivationServiceMock();
        }

        public readonly ActorSystemMock System;
        public readonly TimerServiceMock Timers;
        public readonly ReminderServiceMock Reminders;
        public readonly ActivationServiceMock Activation;

        IActorSystem IActorRuntime.System => System;
        ITimerService IActorRuntime.Timers => Timers;
        IReminderService IActorRuntime.Reminders => Reminders;
        IActivationService IActorRuntime.Activation => Activation;
    }
}
