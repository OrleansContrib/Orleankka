using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka
{
    public class StreamSubscription
    {
        readonly StreamSubscriptionHandle<object> handle;

        protected internal StreamSubscription(StreamSubscriptionHandle<object> handle)
        {
            this.handle = handle;
        }

        public virtual Task Unsubscribe() => handle.UnsubscribeAsync();
    }
}