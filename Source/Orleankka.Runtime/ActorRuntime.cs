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

    public sealed class ActorRuntime : IActorRuntime
    {
        internal ActorRuntime(IActorSystem system, ActorEndpoint endpoint)
        {
            System = system;
            Timers = new TimerService(endpoint);
            Reminders = new ReminderService(endpoint);
            Activation = new ActivationService(endpoint);
        }

        public IActorSystem System { get; }
        public ITimerService Timers { get; }
        public IReminderService Reminders { get; }
        public IActivationService Activation { get; }
    }
}
