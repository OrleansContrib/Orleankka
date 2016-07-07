using Orleankka.Services;

namespace Orleankka.TestKit
{
    public class ActorContextMock : IActorContext
    {
        public ActorContextMock(ActorPath path)
        {
            Path        = path;
            Self        = new ActorRefMock(path);
            System      = new ActorSystemMock();
            Timers      = new TimerServiceMock();
            Reminders   = new ReminderServiceMock();
            Activation  = new ActivationServiceMock();
        }

        public ActorPath Path { get;}
        public readonly ActorRefMock Self;
        public readonly ActorSystemMock System;
        public readonly TimerServiceMock Timers;
        public readonly ReminderServiceMock Reminders;
        public readonly ActivationServiceMock Activation;

        ActorRef IActorContext.Self => Self;
        IActorSystem IActorContext.System => System;
        ITimerService IActorContext.Timers => Timers;
        IReminderService IActorContext.Reminders => Reminders;
        IActivationService IActorContext.Activation => Activation;
    }
}
