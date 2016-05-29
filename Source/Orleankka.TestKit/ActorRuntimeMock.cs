using Orleankka.Core;
using Orleankka.Services;

namespace Orleankka.TestKit
{
    public class ActorRuntimeMock : IActorRuntime
    {
        public ActorRuntimeMock()
        {
            System      = new ActorSystemMock();
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
