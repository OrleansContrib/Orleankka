using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    [Serializable]
    public class Write
    {
        public int Value;
        public TimeSpan Delay;
    }

    [Serializable]
    public class Read
    {}

    [Reentrant(typeof(Read))]
    public class ReaderWriterLock : Actor
    {
        int value;
        ConsolePosition indicator;

        public override Task OnActivate()
        {
            Console.Write("\nWrites: ");
            indicator = ConsolePosition.Current();
            return base.OnActivate();
        }

        public async Task On(Write req)
        {
            value = req.Value;
            indicator.Write(value);
            await Task.Delay(req.Delay);
        }

        public int On(Read req)
        {
            return value;
        }
    }
}