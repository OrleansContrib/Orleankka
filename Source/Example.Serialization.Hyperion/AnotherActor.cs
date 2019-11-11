using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

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
        public StreamRef Stream;
        public Item Item;
    }

    public interface IAnotherActor : IActor
    { }

    public class AnotherActor : Actor, IAnotherActor
    {
        void On(Notify x) => x.Observer.Notify(x.Item);
        Task On(Push x) => x.Stream.Push(x.Item);
    }
}