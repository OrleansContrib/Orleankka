using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;
    using Behaviors;
    using Services;
    using Utility;

    public abstract class Actor
    {
        ActorRef self;
        
        protected Actor()
        {
            Behavior = ActorBehavior.Null(this);
        }

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(Dispatcher dispatcher = null)
            : this(null, null, dispatcher)
        {}

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(IActorRuntime runtime = null, Dispatcher dispatcher = null) 
            : this(null, runtime, dispatcher)
        {}

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(string id = null, Dispatcher dispatcher = null) 
            : this(id, null, dispatcher)
        {}

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected Actor(string id = null, IActorRuntime runtime = null, Dispatcher dispatcher = null) : this()
        {
            Runtime = runtime;
            Dispatcher = dispatcher ?? ActorType.Dispatcher(GetType());
            Path = GetType().ToActorPath(id ?? Guid.NewGuid().ToString("N"));
        }

        internal virtual void Initialize(IActorHost host, ActorPath path, IActorRuntime runtime, Dispatcher dispatcher)
        {
            Path = path;
            Runtime = runtime;
            Dispatcher = Dispatcher ?? dispatcher;
            Host = host;
        }

        public string Id => Path.Id;
        internal IActorHost Host        {get; private set;}

        public ActorPath Path           {get; private set;}
        public IActorRuntime Runtime    {get; private set;}
        public ActorBehavior Behavior   {get; private set;}
        public Dispatcher Dispatcher    {get; private set;}
        
        public IActorSystem System           => Runtime.System;
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders    => Runtime.Reminders;
        public ITimerService Timers          => Runtime.Timers;

        public ActorRef Self => self ?? (self = System.ActorOf(Path));

        public virtual Task OnActivate() => Behavior.HandleActivate();
        public virtual Task OnDeactivate() => Behavior.HandleDeactivate();

        public virtual Task<object> OnReceive(object message) => Behavior.HandleReceive(message);
        public virtual Task OnReminder(string id) => Behavior.HandleReminder(id);

        public async Task<TResult> Dispatch<TResult>(object message, Func<object, Task<object>> fallback = null) => 
            (TResult)await Dispatch(message, fallback);

        public Task<object> Dispatch(object message, Func<object, Task<object>> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            return Dispatcher.Dispatch(this, message, fallback);
        }

        public virtual Task<object> OnUnhandledReceive(RequestOrigin origin, object message) =>
            throw new UnhandledMessageException(this, message);

        public virtual Task OnUnhandledReminder(string id) =>
            throw new UnhandledReminderException(this, id);

        public virtual Task OnTransitioning(Transition transition) => Task.CompletedTask;
        public virtual Task OnTransitioned(Transition transition) => Task.CompletedTask;
        public virtual Task OnTransitionFailure(Transition transition, Exception exception) => Task.CompletedTask;
    }
}