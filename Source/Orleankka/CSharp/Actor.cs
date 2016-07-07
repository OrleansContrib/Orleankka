using System;
using System.Threading.Tasks;

namespace Orleankka.CSharp
{
    using Services;
    using Utility;

    public abstract class Actor
    {
        static readonly Task<object> Done = Task.FromResult((object)null);
        static Task<object> Ignore(object x) => Done;

        protected Actor()
        {}

        protected Actor(IActorContext context)
        {
            Requires.NotNull(context, nameof(context));
            Context = context;
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

        public virtual Task<object> OnReceive(object message)
        {
            if (message is Activate)
                return Dispatch(message, Ignore);

            if (message is Deactivate)
                return Dispatch(message, Ignore);

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