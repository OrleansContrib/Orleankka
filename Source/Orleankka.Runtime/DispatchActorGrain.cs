using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka
{
    using System;
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

        internal override void Initialize(IServiceProvider services, string id)
        {
            base.Initialize(services, id);

            if (dispatcher != null) 
                return;

            var registry = services.GetService<IDispatcherRegistry>();
            dispatcher = registry?.GetDispatcher(GetType());
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