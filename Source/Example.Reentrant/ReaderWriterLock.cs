using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

using Orleans.CodeGeneration;
using Orleans.Concurrency;

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

    public interface IReaderWriterLock : IActor
    { }

    [MayInterleave(nameof(IsReentrant))]
    public class ReaderWriterLock : Actor, IReaderWriterLock
    {
        public static bool IsReentrant(InvokeMethodRequest req) => req.Message(x => x is Read);
        
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