using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    using Cluster;
    using Services;

    public abstract class ActorGrain : Grain, IActorGrain, IGrainWithStringKey
    {
        /// <summary>
        /// Returns actual <see cref="IActorGrain"/> interface implemented by give actor class
        /// </summary>
        /// <param name="type">The actor class type</param>
        /// <returns>Type of the interface</returns>
        public static Type InterfaceOf(Type type) => ActorGrainInterface.InterfaceOf(type);

        public static readonly Done Done = Done.Result;
        public static readonly Unhandled Unhandled = Unhandled.Result;

        public static Task<object> Result(object value) => Task.FromResult(value);

        IActorMiddleware middleware;
        ActorRef self;

        public ActorRef Self => self ??= System.ActorOf(Path);

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
            var @interface = InterfaceOf(GetType());
            Path = ActorPath.For(@interface, id ?? Guid.NewGuid().ToString("N"));
            System = runtime?.System;
            this.runtime = runtime;
        }

        /// <summary>
        /// Run-once initialization routine invoked right after actor grain construction and just before activate
        /// TODO: this is not ideal since IActorRuntime cannot be created here. Still, need support for IActivationFilter from Orleans
        /// </summary>
        internal virtual void Initialize(IServiceProvider services, string id)
        {
            var clusterSystem = services.GetRequiredService<ClusterActorSystem>();
            System = clusterSystem;

            var implementation = clusterSystem.ImplementationOf(GetType());
            middleware = implementation.Middleware;

            Path = ActorPath.For(implementation.Interface, id);
        }

        public string Id => Path.Id;

        public ActorPath Path { get; private set; }
        public IActorSystem System { get; private set; }

        internal IActorRuntime runtime;
        public IActorRuntime Runtime => runtime ??= new ActorRuntime(System, this);        
        public IActivationService Activation => Runtime.Activation;
        public IReminderService Reminders => Runtime.Reminders;
        public ITimerService Timers => Runtime.Timers;
        public IBackgroundJobService Jobs => Runtime.Jobs;

        Task<object> IActorGrain.ReceiveAsk(object message) => ReceiveRequest(message);
        Task IActorGrain.ReceiveTell(object message) => ReceiveRequest(message);
        Task IActorGrain.ReceiveNotify(object message) => ReceiveRequest(message);

        internal Task<object> ReceiveRequest(object message)
        {
            switch (message)
            {
                case Activate _:
                    return TaskResult.Done;
                case Deactivate _:
                    Activation.DeactivateOnIdle();
                    return TaskResult.Done;
                default:
                    return middleware.Receive(this, message, Receive);
            }
        }

        Task IRemindable.ReceiveReminder(string name, TickStatus status) =>
            middleware.Receive(this, new Reminder(name, status), Receive);

        public override Task OnDeactivateAsync() =>
            middleware.Receive(this, Deactivate.Message, Receive);

        public override Task OnActivateAsync() => 
            middleware.Receive(this, Activate.Message, Receive);

        public abstract Task<object> Receive(object message);
    }
}