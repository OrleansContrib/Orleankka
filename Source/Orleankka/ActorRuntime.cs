using System;
using System.Linq;

namespace Orleankka
{
    using Core;
    using Services;

    public interface IActorRuntime
    {
        IActorSystem System             { get; }
        ITimerService Timers            { get; }
        IReminderService Reminders      { get; }
        IActivationService Activation   { get; }
    }

    class ActorRuntime : IActorRuntime
    {
        internal ActorRuntime(IActorSystem system, ActorEndpoint endpoint)
        {
            System      = system;
            Timers      = new TimerService(endpoint);
            Reminders   = new ReminderService(endpoint);
            Activation  = new ActivationService(endpoint);
        }

        public IActorSystem System              { get; private set; }
        public ITimerService Timers             { get; private set; }
        public IReminderService Reminders       { get; private set; }
        public IActivationService Activation    { get; private set; }
    }
}
