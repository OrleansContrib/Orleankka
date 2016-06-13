namespace Orleankka
{
    using Services;

    public interface IActorRuntime
    {
        IActorSystem System             { get; }
        ITimerService Timers            { get; }
        IReminderService Reminders      { get; }
        IActivationService Activation   { get; }
    }

    public class ActorRuntime : IActorRuntime
    {
        internal ActorRuntime(IActorContext context)
        {
            System      = context.System;
            Timers      = context.Timers;
            Reminders   = context.Reminders;
            Activation  = context.Activation;
        }

        public IActorSystem System              { get; }
        public ITimerService Timers             { get; }
        public IReminderService Reminders       { get; }
        public IActivationService Activation    { get; }
    }
}
