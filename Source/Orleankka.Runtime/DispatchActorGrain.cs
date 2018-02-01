using System.Threading.Tasks;

namespace Orleankka
{
    public abstract class DispatchActorGrain : ActorGrain
    {
        Dispatcher Dispatcher { get; set; }

        /// <inheritdoc />
        protected DispatchActorGrain(IActorRuntime runtime)
            : base(runtime)
        {}

        /// <inheritdoc />
        protected DispatchActorGrain(string id)
            : base(id)
        {}

        /// <inheritdoc />
        protected DispatchActorGrain(string id = null, IActorRuntime runtime = null)
            : base(id, runtime)
        {
            Dispatcher = Dispatcher.For(GetType());
        }

        public override Task OnActivateAsync()
        {
            Dispatcher = Dispatcher.For(GetType());
            return base.OnActivateAsync();
        }

        public override Task<object> Receive(object message) => Dispatch(message);

        Task<object> Dispatch(object message) => Dispatcher.DispatchAsync(this, message, x =>
        {
            if (x is LifecycleMessage)
                return Result(Done);

            throw new UnhandledMessageException(this, message);
        });
    }
}