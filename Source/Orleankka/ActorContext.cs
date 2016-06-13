using Orleankka.Core;
using Orleankka.Services;

namespace Orleankka
{
    public interface IActorContext
    {
        ActorPath Path { get; }
        ActorRef Self { get; }
        IActorSystem System { get; }
        ITimerService Timers { get; }
        IReminderService Reminders { get; }
        IActivationService Activation { get; }
    }

    public class ActorContext : IActorContext
    {
        ActorRef self;

        internal ActorContext(ActorPath path, IActorSystem system, ActorEndpoint endpoint)
        {
            Path = path;
            System = system;
            Timers = new TimerService(endpoint);
            Reminders = new ReminderService(endpoint);
            Activation = new ActivationService(endpoint);
        }

        public ActorPath Path { get; }
        public ActorRef Self => self ?? (self = System.ActorOf(Path));
        public IActorSystem System { get; }
        public ITimerService Timers { get; }
        public IReminderService Reminders { get; }
        public IActivationService Activation { get; }
    }
}