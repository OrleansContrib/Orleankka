using System.Threading.Tasks;

using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleankka
{
    public abstract class ActorStorageProvider<TState> : IStorageProvider where TState : new()
    {
        const int FunNamespaceLength = 4;

        string name;

        Task IProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            this.name = name;
            return Init(providerRuntime, config);
        }

        public virtual Task Init(IProviderRuntime runtime, IProviderConfiguration config) => Task.CompletedTask;

        public virtual Task Close() => Task.CompletedTask;

        public string Name => name;

        Task IStorageProvider.ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => 
            ReadStateAsync(grainType.Substring(FunNamespaceLength), grainReference.GetPrimaryKeyString(), (TState) grainState.State);

        public abstract Task ReadStateAsync(string type, string id, TState state);

        Task IStorageProvider.WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => 
            WriteStateAsync(grainType.Substring(FunNamespaceLength), grainReference.GetPrimaryKeyString(), (TState)grainState.State);

        public abstract Task WriteStateAsync(string type, string id, TState state);

        Task IStorageProvider.ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState) => 
            ClearStateAsync(grainType.Substring(FunNamespaceLength), grainReference.GetPrimaryKeyString(), (TState)grainState.State);

        public abstract Task ClearStateAsync(string type, string id, TState state);
        
        Logger IStorageProvider.Log => null;
    }
}