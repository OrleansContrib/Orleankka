using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    public class StreamSubscriptionMock : StreamSubscription
    {
        public object Callback      { get; }
        public StreamFilter Filter  { get; }
        public bool Unsubscribed    { get; private set; }

        public StreamSubscriptionMock(object callback, StreamFilter filter)
            : base(null)
        {
            Callback = callback;
            Filter = filter;
        }

        public override Task Unsubscribe()
        {
            Unsubscribed = false; 
            return TaskDone.Done;
        }
    }
}