using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    using Cluster;
    using Services;

    public abstract class ActorGrain : Grain, IRemindable, IActor
    {
        public static readonly Done Done = Done.Result;
        public static readonly Unhandled Unhandled = Unhandled.Result;

        public static Task<object> Result(object value) => Task.FromResult(value);

        IActorMiddleware middleware;
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
            var @interface = ActorGrainImplementation.InterfaceOf(GetType());
            Path = ActorPath.For(@interface, id ?? Guid.NewGuid().ToString("N"));
            Runtime = runtime;
        }

        public string Id => Path.Id;

        public ActorPath Path { get; private set; }
        public IActorRuntime Runtime { get; private set; }
        
        public IActorSystem System => Runtime.System;
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders => Runtime.Reminders;
        public ITimerService Timers => Runtime.Timers;

        Task<object> IActor.ReceiveAsk(object message) => ReceiveRequest(message);
        Task IActor.ReceiveTell(object message) => ReceiveRequest(message);
        Task IActor.ReceiveNotify(object message) => ReceiveRequest(message);

        internal Task<object> ReceiveRequest(object message) => 
            middleware.Receive(this, message, Receive);

        Task IRemindable.ReceiveReminder(string name, TickStatus status) =>
            middleware.Receive(this, Reminder.Message(name, status), Receive);

        public override Task OnDeactivateAsync() =>
            middleware.Receive(this, Deactivate.Message, Receive);

        public override Task OnActivateAsync()
        {
            var system = ServiceProvider.GetRequiredService<ClusterActorSystem>();

            var implementation = system.ImplementationOf(GetType());
            middleware = implementation.Middleware;

            Path = ActorPath.For(implementation.Interface, this.GetPrimaryKeyString());
            Runtime = new ActorRuntime(system, this);
            
            return middleware.Receive(this, Activate.Message, Receive);
        }

        public abstract Task<object> Receive(object message);
    }
}