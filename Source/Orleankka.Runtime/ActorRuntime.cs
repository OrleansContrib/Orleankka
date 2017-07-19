using Orleans;

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

    public sealed class ActorRuntime : IActorRuntime
    {
        internal ActorRuntime(IActorSystem system, Grain grain)
        {
            System = system;
            Timers = new TimerService(grain);
            Reminders = new ReminderService(grain);
            Activation = new ActivationService(grain);
        }

        public IActorSystem System { get; }
        public ITimerService Timers { get; }
        public IReminderService Reminders { get; }
        public IActivationService Activation { get; }
    }
}
