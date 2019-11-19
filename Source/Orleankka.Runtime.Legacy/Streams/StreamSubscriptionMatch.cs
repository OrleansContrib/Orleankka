using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka.Legacy.Streams
{
    class StreamSubscriptionMatch
    {
        public static readonly StreamSubscriptionMatch None = new StreamSubscriptionMatch();

        StreamSubscriptionMatch()
        {}

        public readonly string Target;
        public readonly Func<object, Task> Receiver;
        public readonly Func<object, bool> Filter;

        public StreamSubscriptionMatch(string target, Func<object, Task> receiver, Func<object, bool> filter)
        {
            Target = target;
            Receiver = receiver;
            Filter = filter;
        }

        public Task Receive(object item) => Filter(item) ? Receiver(item) : Task.CompletedTask;
    }
}