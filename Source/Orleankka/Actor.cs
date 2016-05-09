using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;
    using Services;
    using Utility;

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

        internal void Initialize(ActorType type, string id, IActorRuntime runtime)
        {
            Id = id;
            Type = type;
            Runtime = runtime;
        }

        public string Id
        {
            get; private set;
        }

        public IActorRuntime Runtime
        {
            get; set;
        }

        internal ActorType Type
        {
            get; set;
        }

        internal ActorInterface Interface => Type.Interface;
        internal ActorImplementation Implementation => Type.Implementation;

        public ActorRef Self => self ?? (self = System.ActorOf(GetType(), Id));

        public IActorSystem System           => Runtime.System;
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders    => Runtime.Reminders;
        public ITimerService Timers          => Runtime.Timers;

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
            return Implementation.Dispatch(this, message, fallback);
        }
    }
}