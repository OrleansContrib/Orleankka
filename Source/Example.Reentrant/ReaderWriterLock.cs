using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

namespace Example
{
    [Serializable]
    public class Write : Command
    {
        public int Value;
        public TimeSpan Delay;
    }

    [Serializable]
    public class Read : Query<int>
    {}

    public interface IReaderWriterLock : IActorGrain
    {}

    [Interleave(typeof(Read))]
    public class ReaderWriterLock : ActorGrain, IReaderWriterLock
    {
        int value;
        ConsolePosition indicator;

        public override Task OnActivate()
        {
            Console.Write("\nWrites: ");
            indicator = ConsolePosition.Current();
            return base.OnActivate();
        }

        async Task On(Write req)
        {
            value = req.Value;
            indicator.Write(value);
            await Task.Delay(req.Delay);
        }

        int On(Read req) => value;
    }
}