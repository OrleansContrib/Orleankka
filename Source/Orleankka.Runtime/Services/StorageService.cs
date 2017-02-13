using System.Threading.Tasks;

namespace Orleankka.Services
{
    using Core;

    /// <summary>
    /// Manages state of a stateful actor
    /// </summary>
    public interface IStorageService<out TState> where TState : new()
    {
        TState State { get; }
        Task ClearState();
        Task WriteState();
        Task ReadState();
    }

    /// <summary>
    /// Default runtime-bound implementation of <see cref="IStorageService{TState}"/>
    /// </summary>
    class StorageService<TState> : IStorageService<TState> where TState: new()
    {
        readonly StatefulActorEndpoint<TState> endpoint;

        public StorageService(StatefulActorEndpoint<TState> endpoint)
        {
            this.endpoint = endpoint;
        }

        TState IStorageService<TState>.State => endpoint.State;
        Task IStorageService<TState>.ClearState() => endpoint.ClearStateAsync();
        Task IStorageService<TState>.WriteState() => endpoint.WriteStateAsync();
        Task IStorageService<TState>.ReadState() => endpoint.ReadStateAsync();
    }
}
