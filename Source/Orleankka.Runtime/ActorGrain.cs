using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Orleankka
{
    using Behaviors;
    using Core;
    using Services;
    using Utility;

    public delegate Task<object> Receive(object message);

    public interface ReceiveResult
    {}
	
    [Serializable]
    public sealed class Done : ReceiveResult
    {
        public static readonly Done Message = new Done();
    }

    [Serializable]
    public sealed class Unhandled : ReceiveResult
    {
        public static readonly Unhandled Message = new Unhandled();
    }       

    public abstract class ActorGrain : Grain, IRemindable, IActor
    {
        public static readonly Done Done = Done.Message;
        public static readonly Unhandled Unhandled = Unhandled.Message;

        public static Task<object> Result(object value) => Task.FromResult(value);

        ActorType actor;
        ActorType Actor => actor ?? (actor = ActorType.Of(GetType()));

        ActorRef self;
        public ActorRef Self => self ?? (self = System.ActorOf(Path));

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
        protected ActorGrain(string id = null, IActorRuntime runtime = null)
        {
            Runtime = runtime;
            Dispatcher = ActorType.Dispatcher(GetType());
            Path = GetType().ToActorPath(id ?? Guid.NewGuid().ToString("N"));
        }

        public string Id => Path.Id;

        public ActorPath Path           {get; private set;}
        public IActorRuntime Runtime    {get; private set;}
        Dispatcher Dispatcher           {get; set;}
        
        public IActorSystem System           => Runtime.System;
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders    => Runtime.Reminders;
        public ITimerService Timers          => Runtime.Timers;

        Task<object> IActor.ReceiveAsk(object message) => ReceiveRequest(message);
        Task IActor.ReceiveTell(object message) => ReceiveRequest(message);
        Task IActor.ReceiveNotify(object message) => ReceiveRequest(message);

        internal Task<object> ReceiveRequest(object message) => 
            Actor.Middleware.Receive(this, message, Receive);

        Task IRemindable.ReceiveReminder(string name, TickStatus status) => 
            Actor.Middleware.Receive(this, Reminder.Message(name, status), Receive);

        public override Task OnDeactivateAsync() => 
            Actor.Middleware.Receive(this, Deactivate.Message, Receive);

        public override Task OnActivateAsync()
        {
            Path = ActorPath.From(Actor.Name, this.GetPrimaryKeyString());
            Runtime = new ActorRuntime(ServiceProvider.GetRequiredService<IActorSystem>(), this);
            Dispatcher = ActorType.Dispatcher(GetType());

            return Actor.Middleware.Receive(this, Activate.Message, Receive);
        }

        public async Task<object> Receive(object message)
        {
            var response = await OnReceive(message);

            switch (response)
            {
                case Task _:
                    throw new InvalidOperationException(
                        $"Actor '{GetType().Name}:{Id}' tries to return Task in response to '{message.GetType()}' message");
                case Done _:
                    return null;
                case Unhandled _:
                    return await OnUnhandledReceive(message);
            }

            return response;
        }

        protected virtual Task<object> OnReceive(object message) => Dispatch(message);

        public virtual Task<object> OnUnhandledReceive(object message) => 
            throw new UnhandledMessageException(this, message);
        
        internal Task<object> Dispatch(object message)
        {
            Requires.NotNull(message, nameof(message));

            return Dispatcher.DispatchAsync(this, message, x => 
                x is LifecycleMessage ? Result(Done) : Result(Unhandled));
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

        public interface BehaviorMessage
        {}

        public sealed class Become : BehaviorMessage, LifecycleMessage
        {
            public static readonly Become Message = new Become();
        }

        public sealed class Unbecome : BehaviorMessage, LifecycleMessage
        {
            public static readonly Unbecome Message = new Unbecome();
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