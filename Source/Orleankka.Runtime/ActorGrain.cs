using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using System;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;
    using Services;
    using Utility;

    public delegate Task<object> Receive(object message);

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
        { }

        /// <inheritdoc />
        protected ActorGrain(string id)
            : this(id, null)
        { }

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

        public ActorPath Path { get; private set; }
        public IActorRuntime Runtime { get; private set; }
        Dispatcher Dispatcher { get; set; }

        public IActorSystem System => Runtime.System;
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders => Runtime.Reminders;
        public ITimerService Timers => Runtime.Timers;

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
            return await OnReceive(message);
        }

        protected virtual Task<object> OnReceive(object message) => Dispatch(message);

        Task<object> Dispatch(object message)
        {
            Requires.NotNull(message, nameof(message));

            return Dispatcher.DispatchAsync(this, message, x =>
            {
                if (x is LifecycleMessage)
                    return Result(Done);

                throw new UnhandledMessageException(this, message);
            });
        }
}