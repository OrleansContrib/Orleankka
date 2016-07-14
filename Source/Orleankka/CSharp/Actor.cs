using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.CSharp
{
    using Services;
    using Utility;

    public abstract class Actor
    {
        protected Actor()
        {}

        protected Actor(IActorContext context)
        {
            Requires.NotNull(context, nameof(context));
            Context = context;
            Dispatcher = ActorBinding.Dispatcher(GetType());
        }

        protected Actor(IActorContext context, Dispatcher dispatcher)
        {
            Requires.NotNull(context, nameof(context));
            Requires.NotNull(dispatcher, nameof(dispatcher));
            Context = context;
            Dispatcher = dispatcher;
        }

        internal void Initialize(IActorContext context, Dispatcher dispatcher)
        {
            Context = context;
            Dispatcher = dispatcher;
        }

        public IActorContext Context { get; private set; }
        public Dispatcher Dispatcher { get; private set; }

        public ActorRef Self => Context.Self;
        public ActorPath Path => Context.Path;
        public string Code => Path.Code;
        public string Id => Path.Id;

        public IActorSystem System           => Context.System;
        public IActivationService Activation => Context.Activation;
        public IReminderService Reminders    => Context.Reminders;
        public ITimerService Timers          => Context.Timers;

        public virtual Task<object> OnReceive(object message) => 
            Dispatch(message);

        public virtual Task OnActivate()    => TaskDone.Done;
        public virtual Task OnDeactivate()  => TaskDone.Done;

        public virtual Task OnReminder(string id)
        {
            var message = $"Override {"OnReminder"}() method in class {GetType()} to implement corresponding behavior";
            throw new NotImplementedException(message);
        }

        public virtual Task OnTimer(string id, object state)
        {
            var message = $"Override {"OnTimer"}() method in class {GetType()} to implement corresponding behavior";
            throw new NotImplementedException(message);
        }

        public async Task<TResult> Dispatch<TResult>(object message, Func<object, Task<object>> fallback = null) => 
            (TResult)await Dispatch(message, fallback);

        public Task<object> Dispatch(object message, Func<object, Task<object>> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            return Dispatcher.Dispatch(this, message, fallback);
        }
    }
}