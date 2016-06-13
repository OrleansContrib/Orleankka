using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

namespace Orleankka
{
    using Utility; 

    public class StreamSubscription
    {
        readonly StreamSubscriptionHandle<object> handle;

        protected internal StreamSubscription(StreamSubscriptionHandle<object> handle)
        {
            this.handle = handle;
        }

        public virtual Task Unsubscribe()
        {
            return handle.UnsubscribeAsync();
        }

        public virtual Task Resume(Func<object, Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));
            var observer = new StreamRef.Observer((item, token) => callback(item));
            return handle.ResumeAsync(observer);
        }

        public virtual Task Resume<T>(Func<T, Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));
            return Resume(item => callback((T)item));
        }

        public virtual Task Resume(Action<object> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return Resume(item =>
            {
                callback(item);
                return TaskDone.Done;
            });
        }

        public virtual Task Resume<T>(Action<T> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return Resume(item =>
            {
                callback((T)item);
                return TaskDone.Done;
            });
        }
    }
}