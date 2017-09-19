using System;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    public class StreamSubscriptionMock : StreamSubscription
    {
        public object Callback      { get; private set; }
        public StreamFilter Filter  { get; private set; }
        public bool Resumed         { get; private set; }
        public bool Unsubscribed    { get; private set; }

        public StreamSubscriptionMock(object callback, StreamFilter filter)
            : base(null)
        {
            Callback = callback;
            Filter = filter;
        }

        public override Task<StreamSubscription> Resume(Func<object, Task> callback)
        {
            Resumed = true;
            Callback = callback;
            return Task.FromResult((StreamSubscription)this);
        }

        public override Task<StreamSubscription> Resume<T>(Func<T, Task> callback)
        {
            Resumed = true;
            Callback = callback;
            return Task.FromResult((StreamSubscription)this);
        }

        public override Task<StreamSubscription> Resume(Action<object> callback)
        {
            Resumed = true;
            Callback = callback;
            return Task.FromResult((StreamSubscription)this);
        }

        public override Task<StreamSubscription> Resume<T>(Action<T> callback)
        {
            Resumed = true;
            Callback = callback;
            return Task.FromResult((StreamSubscription)this);
        }

        public override Task Unsubscribe()
        {
            Unsubscribed = false; 
            return TaskDone.Done;
        }
    }
}