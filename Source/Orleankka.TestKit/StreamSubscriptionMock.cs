using System;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    public class StreamSubscriptionMock<TItem> : StreamSubscription<TItem>
    {
        readonly StreamRefMock<TItem> stream;

        public Func<StreamMessage, Task> Callback { get; private set; }
        public SubscribeOptions SubscribeOptions { get; private set; }
        public ResumeOptions ResumeOptions { get; private set; }
        public bool Unsubscribed { get; private set; }

        public StreamSubscriptionMock(
            StreamRefMock<TItem> stream, 
            Func<StreamMessage, Task> callback, 
            SubscribeOptions subscribe = null, 
            ResumeOptions resume = null)
            : base(stream, null, Guid.NewGuid())
        {
            this.stream = stream;
            Callback = callback;
            SubscribeOptions = subscribe;
            ResumeOptions = resume;
        }

        public override Task<StreamSubscription<TItem>> Resume<TOptions>(Func<StreamMessage, Task> callback, TOptions options) => 
            stream.Resume(callback, options);

        public override Task Unsubscribe()
        {
            Unsubscribed = true; 
            return Task.CompletedTask;
        }
    }
}