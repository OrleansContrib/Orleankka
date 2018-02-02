using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka
{
    using Utility;

    public abstract class DispatchActorGrain : ActorGrain
    {
        Dispatcher dispatcher;
        public Dispatcher Dispatcher => dispatcher ?? (dispatcher = new Dispatcher(GetType()));

        /// <inheritdoc />
        protected DispatchActorGrain(IActorRuntime runtime)
            : base(runtime)
        {}

        /// <inheritdoc />
        protected DispatchActorGrain(string id)
            : base(id)
        {}

        /// <inheritdoc />
        protected DispatchActorGrain(Dispatcher dispatcher)
        {
            Requires.NotNull(dispatcher, nameof(dispatcher));
            this.dispatcher = dispatcher;
        }

        /// <inheritdoc />
        protected DispatchActorGrain(string id = null, IActorRuntime runtime = null, Dispatcher dispatcher = null)
            : base(id, runtime)
        {
            this.dispatcher = dispatcher;
        }

        public override Task OnActivateAsync()
        {
            if (dispatcher == null)
            {
                var registry = ServiceProvider?.GetService<IDispatcherRegistry>();
                dispatcher = registry?.GetDispatcher(GetType());
            }

            return base.OnActivateAsync();
        }

        public override Task<object> Receive(object message) => Dispatch(message);

        Task<object> Dispatch(object message) => Dispatcher.DispatchResultAsync(this, message, x =>
        {
            if (x is LifecycleMessage)
                return Result(Done);

            throw new UnhandledMessageException(this, message);
        });
    }
}