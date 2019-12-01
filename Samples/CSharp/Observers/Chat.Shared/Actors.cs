using Orleankka;

using Orleans;

namespace Example
{
    public interface IChatRoom : IActorGrain, IGrainWithStringKey
    {}
}