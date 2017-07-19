using System.Threading.Tasks;

using Orleankka.Core;
using Orleankka.Services;

using Orleans;

namespace Orleankka
{
    interface IStatefulActor
    {}

    public abstract class StatefulActor<TState> : Actor, IStatefulActor where TState : new()
    {
        IStorageService<TState> storage;

        protected StatefulActor()
        {}

        protected StatefulActor(string id, IActorRuntime runtime, Dispatcher dispatcher = null, IStorageService<TState> storage = null)
            : base(id, runtime, dispatcher)
        {
            this.storage = storage;
        }

        internal override void Initialize(IActorHost host, ActorPath path, IActorRuntime runtime, Dispatcher dispatcher)
        {
            base.Initialize(host, path, runtime, dispatcher);
            var endpoint = (StatefulActorEndpoint<TState>) host;
            storage = new StorageService<TState>(endpoint);
        }

        public TState State => storage.State;

        public Task ClearState() => storage.ClearState();
        public Task WriteState() => storage.WriteState();
        public Task ReadState() => storage.ReadState();
    }
}
