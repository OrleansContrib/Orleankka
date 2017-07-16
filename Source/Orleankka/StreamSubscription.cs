using System;
using System.Threading.Tasks;

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

        public virtual async Task<StreamSubscription> Resume(Func<object, Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));
            var observer = new StreamRef.Observer((item, token) => callback(item));
            return new StreamSubscription(await handle.ResumeAsync(observer));
        }

        public virtual Task<StreamSubscription> Resume<T>(Func<T, Task> callback)
        {
            Requires.NotNull(callback, nameof(callback));
            return Resume(item => callback((T)item));
        }

        public virtual Task<StreamSubscription> Resume(Action<object> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return Resume(item =>
            {
                callback(item);
                return Task.CompletedTask;
            });
        }

        public virtual Task<StreamSubscription> Resume<T>(Action<T> callback)
        {
            Requires.NotNull(callback, nameof(callback));

            return Resume(item =>
            {
                callback((T)item);
                return Task.CompletedTask;
            });
        }
    }
}