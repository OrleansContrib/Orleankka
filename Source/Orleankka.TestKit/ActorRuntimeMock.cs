using System;
using System.Linq;

using Orleankka.Services;

namespace Orleankka.TestKit
{
    public class ActorRuntimeMock : IActorRuntime
    {
        public readonly ActorSystemMock System = new ActorSystemMock();
        public readonly TimerServiceMock Timers = new TimerServiceMock();
        public readonly ReminderServiceMock Reminders = new ReminderServiceMock();
        public readonly ActivationServiceMock Activation = new ActivationServiceMock();

        IActorSystem IActorRuntime.System
        {
            get { return System; }
        }

        ITimerService IActorRuntime.Timers
        {
            get { return Timers; }
        }

        IReminderService IActorRuntime.Reminders
        {
            get { return Reminders; }
        }

        IActivationService IActorRuntime.Activation
        {
            get { return Activation; }
        }
    }
}
