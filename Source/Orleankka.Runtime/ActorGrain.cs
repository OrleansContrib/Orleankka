using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka
{
    using Core;
    using Services;
    using Utility;

    public interface IActorReceiver
    {
        Task<object> Receive(object message);
    }

    public abstract class ActorGrain : Grain, IRemindable, IActor, IActorReceiver
    {
        public static readonly Task<object> Done = Task.FromResult<object>(0);
        public static Task<object> Result<T>(T value) => Task.FromResult<object>(value);

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

        Task<object> IActorReceiver.Receive(object message) => ReceiveInternal(message);
        Task<object> IActor.ReceiveAsk(object message) => ReceiveInternal(message);
        Task IActor.ReceiveTell(object message) => ReceiveInternal(message);
        Task IActor.ReceiveNotify(object message) => ReceiveInternal(message);

        internal async Task<object> ReceiveInternal(object message)
        {
            Actor.KeepAlive(this);

            var response = await Actor.Middleware.Receive(this, message, Receive);
            if (ReferenceEquals(response, Done))
                return null;

            if (response is Task)
                throw new InvalidOperationException("Can't return Task as actor response");
            
            return response;
        }

        Task IRemindable.ReceiveReminder(string name, TickStatus status)
        {
            Actor.KeepAlive(this);

            return Actor.Middleware.Receive(this, Reminder.Message(name, status), Receive);
        }

        public override Task OnActivateAsync()
        {
            Path = ActorPath.From(Actor.Name, this.GetPrimaryKeyString());
            Runtime = new ActorRuntime(ServiceProvider.GetRequiredService<IActorSystem>(), this);
            Dispatcher = ActorType.Dispatcher(GetType());

            return Actor.Middleware.Receive(this, Activate.Message, Receive);
        }

        public override Task OnDeactivateAsync()
        {
            return Actor.Middleware.Receive(this, Deactivate.Message, Receive);
        }

        public virtual Task<object> Receive(object message) => Dispatch(message);

        public async Task<TResult> Dispatch<TResult>(object message) => (TResult) await Dispatch(message);

        public Task<object> Dispatch(object message)
        {
            Requires.NotNull(message, nameof(message));

            return Dispatcher.Dispatch(this, message, x =>
            {
                if (x is LifecycleMessage)
                    return Done;

                throw new Dispatcher.HandlerNotFoundException(this, x.GetType());
            });
        }

        public interface LifecycleMessage
        {}

        public sealed class Activate : LifecycleMessage
        {
            public static readonly Activate Message = new Activate();
        }

        public sealed class Deactivate : LifecycleMessage
        {
            public static readonly Deactivate Message = new Deactivate();
        }

        public struct Reminder
        {
            public static readonly Reminder Invalid = 
                new Reminder();

            public static Reminder Message(string name, TickStatus status) => 
                new Reminder(name, status);

            public string Name { get; }
            public TickStatus Status { get; }

            public Reminder(string name, TickStatus status = default(TickStatus))
            {
                Name = name;
                Status = status;
            }
        }
    }
}