using System;
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

        protected Actor(string id, IActorRuntime runtime, Dispatcher dispatcher = null) : this()
        {
            Requires.NotNull(runtime, nameof(runtime));
            Requires.NotNullOrWhitespace(id, nameof(id));

            Runtime = runtime;
            Dispatcher = dispatcher ?? ActorType.Dispatcher(GetType());
            Path = GetType().ToActorPath(id);
        }

        internal void Initialize(ActorEndpoint endpoint, ActorPath path, IActorRuntime runtime, Dispatcher dispatcher)
        {
            Path = path;
            Runtime = runtime;
            Dispatcher = Dispatcher ?? dispatcher;
            Endpoint = endpoint;
        }

        public string Id => Path.Id;
        internal ActorEndpoint Endpoint {get; private set;}

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
    }
}