using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

using Microsoft.Extensions.DependencyInjection;

using Orleankka.Core;
using Orleankka.Services;
using Orleankka.Utility;

using IReminderService = Orleankka.Services.IReminderService;

namespace Orleankka
{
    public abstract class ActorGrain : Grain, IRemindable, IActor
    {
        ActorType actor;
        ActorType Actor => actor ?? (actor = ActorType.Of(GetType()));

        ActorRef self;
        public ActorRef Self => self ?? (self = System.ActorOf(Path));

        protected ActorGrain()
        {}

        /// <inheritdoc />
        protected ActorGrain(IActorRuntime runtime) 
            : this(null, runtime)
        {}
        
        /// <inheritdoc />
        protected ActorGrain(string id) 
            : this(id, null)
        {}

        /// <summary>
        /// Provided only for unit-testing purposes
        /// </summary>
        protected ActorGrain(string id = null, IActorRuntime runtime = null) : this()
        {
            Runtime = runtime;
            Dispatcher = ActorType.Dispatcher(GetType());
            Path = GetType().ToActorPath(id ?? Guid.NewGuid().ToString("N"));
        }

        public string Id => Path.Id;

        public ActorPath Path           {get; private set;}
        public IActorRuntime Runtime    {get; private set;}
        public Dispatcher Dispatcher    {get; private set;}
        
        public IActorSystem System           => Runtime.System;
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders    => Runtime.Reminders;
        public ITimerService Timers          => Runtime.Timers;

        Task<object> IActor.ReceiveAsk(object message) => ReceiveAsk(message);
        Task IActor.ReceiveTell(object message) => ReceiveAsk(message);
        Task IActor.ReceiveNotify(object message) => ReceiveAsk(message);

        internal Task<object> ReceiveAsk(object message)
        {
            Actor.KeepAlive(this);
            return Actor.Invoker.OnReceive(this, message);
        }

        Task IRemindable.ReceiveReminder(string name, TickStatus status)
        {
            Actor.KeepAlive(this);
            return Actor.Invoker.OnReminder(this, name);
        }

        public override Task OnActivateAsync()
        {
            Path = ActorPath.From(Actor.Name, this.GetPrimaryKeyString());
            Runtime = new ActorRuntime(ServiceProvider.GetRequiredService<IActorSystem>(), this);
            Dispatcher = ActorType.Dispatcher(GetType());

            return Actor.Invoker.OnActivate(this);
        }

        public override Task OnDeactivateAsync() => Actor.Invoker.OnDeactivate(this);

        public virtual Task OnActivate() => Task.CompletedTask;
        public virtual Task OnDeactivate() => Task.CompletedTask;

        public virtual Task<object> OnReceive(object message) => Dispatch(message);
        public virtual Task OnReminder(string id) => throw new NotImplementedException("Override OnReminder(string) method in order to process reminder ticks");

        public async Task<TResult> Dispatch<TResult>(object message, Func<object, Task<object>> fallback = null) => 
            (TResult)await Dispatch(message, fallback);

        public Task<object> Dispatch(object message, Func<object, Task<object>> fallback = null)
        {
            Requires.NotNull(message, nameof(message));
            return Dispatcher.Dispatch(this, message, fallback);
        }
    }
}