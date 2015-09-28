using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;
    using Services;
    using Utility;

    public interface IActor
    {}

    public abstract class Actor : IActor
    {
        ActorRef self;

        protected Actor()
        {}

        protected Actor(string id, IActorRuntime runtime)
        {
            Requires.NotNull(runtime, nameof(runtime));
            Requires.NotNullOrWhitespace(id, nameof(id));

            Id = id;
            Runtime = runtime;
        }

        internal void Initialize(string id, IActorRuntime runtime, ActorPrototype prototype)
        {
            Id = id;
            Runtime = runtime;
            Prototype = prototype;
        }

        protected string Id
        {
            get; private set;
        }

        private IActorRuntime Runtime
        {
            get; set;
        }

        internal ActorPrototype Prototype
        {
            get; set;
        }

        protected ActorRef Self => self ?? (self = System.ActorOf(GetType(), Id));

        protected IActorSystem System           => Runtime.System;
        protected IActivationService Activation => Runtime.Activation;
        protected IReminderService Reminders    => Runtime.Reminders;
        protected ITimerService Timers          => Runtime.Timers;

        public virtual Task OnActivate()    => TaskDone.Done;
        public virtual Task OnDeactivate()  => TaskDone.Done;

        public virtual Task OnReminder(string id)
        {
            var message = $"Override {"OnReminder"}() method in class {GetType()} to implement corresponding behavior";
            throw new NotImplementedException(message);
        }

        public virtual Task<object> OnReceive(object message)
        {
            return Dispatch(message);
        }

        public async Task<TResult> Dispatch<TResult>(object message, Func<object, Task<object>> fallback = null)
        {
            return (TResult)await Dispatch(message, fallback);
        }

        public Task<object> Dispatch(object message, Func<object, Task<object>> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            return Prototype.Dispatch(this, message, fallback);
        }
    }
}