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

        public override Task<object> OnReceive(object message)
        {
            return this.On((dynamic)message);
        }

        public override Task OnActivate()
        {
            Console.Write("\nWrites: ");
            indicator = ConsolePosition.Current();

            return base.OnActivate();
        }

        public async Task<object> On(Write req)
        {
            value = req.Value;
            
            indicator.Write(value);
            await Task.Delay(req.Delay);
            
            return null;
        }

        public Task<object> On(Read req)
        {
            return Result(value);
        }
    }
}