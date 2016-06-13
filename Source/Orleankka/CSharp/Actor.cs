using System;
using System.Threading.Tasks;

using Orleankka.Services;
using Orleankka.Utility;

using Orleans;

namespace Orleankka.CSharp
{
    public abstract class Actor
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

        internal void Initialize(string id, IActorRuntime runtime, Dispatcher dispatcher)
        {
            Id = id;
            Runtime = runtime;
            Dispatcher = dispatcher;
        }

        public string Id
        {
            get; private set;
        }

        public IActorRuntime Runtime
        {
            get; set;
        }

        public Dispatcher Dispatcher
        {
            get; set;
        }

        public ActorRef Self => self ?? (self = System.ActorOf(GetType().ToActorPath(Id)));

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
            return Dispatcher.Dispatch(this, message, fallback);
        }
    }
}