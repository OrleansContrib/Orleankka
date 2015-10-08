using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    public class StreamSubscriptionMock : StreamSubscription
    {
        public object Callback      { get;}
        public bool Unsubscribed    { get; private set; }

        public StreamSubscriptionMock(object callback)
            : base(null)
        {
            Callback = callback;
        }

        public override Task Unsubscribe()
        {
            Unsubscribed = false; 
            return TaskDone.Done;
        }
    }
}