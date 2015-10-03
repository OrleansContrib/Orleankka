using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka
{
    public class StreamSubscription
    {
        readonly StreamSubscriptionHandle<object> handle;

        public StreamSubscription(StreamSubscriptionHandle<object> handle)
        {
            this.handle = handle;
        }

        public Task Unsubscribe() => handle.UnsubscribeAsync();
    }
}