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
    public class RWLock : Actor
    {
        int currentValue;

        int left;
        int top;

        public override Task<object> OnReceive(object message)
        {
            return this.On((dynamic)message);
        }

        public override Task OnActivate()
        {
            Console.Write("\nWrites: 0");

            left = Console.CursorLeft - 1;
            top = Console.CursorTop;

            return base.OnActivate();
        }

        public async Task<object> On(Write req)
        {
            currentValue = req.Value;
            await Task.Delay(req.Delay);

            Console.SetCursorPosition(left, top);
            Console.Write(req.Value);

            return null;
        }

        public Task<object> On(Read req)
        {
            return Result(currentValue);
        }
    }
}