using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

using Orleans;
using Orleans.Concurrency;

namespace Example
{
	[Immutable]
    public class Notify : Command
    {
        public ObserverRef Observer;
        public Item Item;
    }

    [Immutable]
    public class Push : Command
    {
        public StreamRef<Item> Stream;
        public Item Item;
    }

    public interface IAnotherActor : IActorGrain, IGrainWithStringKey 
    {}
    
    public class AnotherActor : DispatchActorGrain, IAnotherActor
    {
        void On(Notify x) => x.Observer.Notify(x.Item);
        Task On(Push x) => x.Stream.Publish(x.Item);
    }
}