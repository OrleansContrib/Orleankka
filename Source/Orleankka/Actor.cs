using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka.Services;
using Orleankka.Utility;

using Orleans;

namespace Orleankka
{
    public interface IActor
    {}

    public abstract class Actor : IActorInvoker
    {
        ActorRef self;

        protected Actor()
        {}

        protected Actor(string id, IActorRuntime runtime, Dispatcher dispatcher = null)
        {
            Requires.NotNull(runtime, nameof(runtime));
            Requires.NotNullOrWhitespace(id, nameof(id));

            Runtime = runtime;
            Dispatcher = dispatcher ?? ActorBinding.Dispatcher(GetType());
            Path = GetType().ToActorPath(id);
        }

        internal void Initialize(ActorPath path, IActorRuntime runtime, Dispatcher dispatcher)
        {
            Path = path;
            Runtime = runtime;
            Dispatcher = Dispatcher ?? dispatcher;
        }

        public string Id => Path.Id;

        public ActorPath Path           {get; private set;}
        public IActorRuntime Runtime    {get; private set;}
        public Dispatcher Dispatcher    {get; private set;}

        public IActorSystem System           => Runtime.System;
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders    => Runtime.Reminders;
        public ITimerService Timers          => Runtime.Timers;

        public ActorRef Self => self ?? (self = System.ActorOf(Path));

        public virtual Task<object> OnReceive(object message) => 
            Dispatch(message);

        public virtual Task OnActivate()    => TaskDone.Done;
        public virtual Task OnDeactivate()  => TaskDone.Done;

        public virtual Task OnReminder(string id)
        {
            var message = $"Override {nameof(OnReminder)}() method in class {GetType()} to implement corresponding behavior";
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