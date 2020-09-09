using Orleans;

namespace Orleankka.Testing
{
    interface ITestActor : IActorGrain, IGrainWithStringKey
    { }

    class TestActor : DispatchActorGrain, ITestActor
    { }
}
