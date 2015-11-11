using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Core.Streams
{
    class StreamSubscriptionMatch
    {
        public static readonly StreamSubscriptionMatch None = new StreamSubscriptionMatch();

        StreamSubscriptionMatch()
        {}

        public readonly Func<object, Task> Receiver;
        public readonly Func<object, bool> Filter;

        public StreamSubscriptionMatch(Func<object, Task> receiver, Func<object, bool> filter)
        {
            Receiver = receiver;
            Filter = filter;
        }

        public Task Receive(object item) => Filter(item) ? Receiver(item) : TaskDone.Done;
    }
}