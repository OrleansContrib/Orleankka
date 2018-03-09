using System;
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

    public interface IReaderWriterLock : IActorGrain
    {}

    [MayInterleave(nameof(Interleave))]
    public class ReaderWriterLock : DispatchActorGrain, IReaderWriterLock
    {
        public static bool Interleave(InvokeMethodRequest req) => req.Message() is Read;

        int value;
        ConsolePosition indicator;

        void On(Activate _)
        {
            Console.Write("\nWrites: ");
            indicator = ConsolePosition.Current();
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