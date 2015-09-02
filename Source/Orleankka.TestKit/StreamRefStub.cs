using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

namespace Orleankka.TestKit
{
    public class StreamRefStub : StreamRef, IStreamIdentity
    {
        private readonly IList<StreamSubscriptionHandle<object>> subscriptionHandles = new List<StreamSubscriptionHandle<object>>();

        public StreamRefStub(StreamPath path)
            : base(path)
        {}

        IEnumerable<StubStreamSubscriptionHandle<object>> StreamSubscriptionHandles
        {
            get { return subscriptionHandles.OfType<StubStreamSubscriptionHandle<object>>(); }
        }

        #region IStreamIdentity Members

        public Guid Guid
        {
            get { return Guid.Empty; }
        }

        public string Namespace
        {
            get { return Path.Id; }
        }

        #endregion

        public override async Task OnCompletedAsync()
        {
            foreach (var streamSubscriptionHandle in StreamSubscriptionHandles)
                await streamSubscriptionHandle.Observer.OnCompletedAsync();
        }

        public override async Task OnNextBatchAsync(IEnumerable<object> batch, StreamSequenceToken token = null)
        {
            foreach (var streamSubscriptionHandle in StreamSubscriptionHandles)
                await streamSubscriptionHandle.Observer.OnNextAsync(batch, token);
        }

        public override async Task OnErrorAsync(Exception ex)
        {
            foreach (var streamSubscriptionHandle in StreamSubscriptionHandles)
                await streamSubscriptionHandle.Observer.OnErrorAsync(ex);
        }

        public override async Task OnNextAsync(object item, StreamSequenceToken token = null)
        {
            foreach (var streamSubscriptionHandle in StreamSubscriptionHandles)
                await streamSubscriptionHandle.Observer.OnNextAsync(item, token);
        }

        public override Task<StreamSubscriptionHandle<object>> SubscribeAsync(
            IAsyncObserver<object> observer,
            StreamSequenceToken token,
            StreamFilterPredicate filterFunc = null,
            object filterData = null)
        {
            throw new NotImplementedException();
        }

        public override Task<IList<StreamSubscriptionHandle<object>>> GetAllSubscriptionHandles()
        {
            return Task.FromResult(subscriptionHandles);
        }

        public override Task<StreamSubscriptionHandle<object>> SubscribeAsync(IAsyncObserver<object> observer)
        {
            var streamSubscriptionHandle = new StubStreamSubscriptionHandle<object>(Guid.NewGuid(), this, observer);

            subscriptionHandles.Add(streamSubscriptionHandle);

            return Task.FromResult(streamSubscriptionHandle as StreamSubscriptionHandle<object>);
        }

        void Unsubscribe(Guid handleId)
        {
            subscriptionHandles.Remove(StreamSubscriptionHandles.First(a => a.HandleId == handleId));
        }

        #region Nested type: StubStreamSubscriptionHandle

        sealed class StubStreamSubscriptionHandle<T> : StreamSubscriptionHandle<T>
        {
            readonly StreamRefStub streamRefStub;
            readonly Guid subscriptionId;
            IAsyncObserver<T> observer;

            internal StubStreamSubscriptionHandle(Guid subscriptionId, StreamRefStub streamRefStub, IAsyncObserver<T> observer)
            {
                this.subscriptionId = subscriptionId;
                this.streamRefStub = streamRefStub;
                this.observer = observer;
            }

            public override IStreamIdentity StreamIdentity
            {
                get { return streamRefStub; }
            }

            internal IAsyncObserver<T> Observer
            {
                get { return observer; }
            }

            public Guid HandleId
            {
                get { return subscriptionId; }
            }

            public override Task UnsubscribeAsync()
            {
                observer = null;
                streamRefStub.Unsubscribe(subscriptionId);

                return TaskDone.Done;
            }

            public override Task<StreamSubscriptionHandle<T>> ResumeAsync(IAsyncObserver<T> observer, StreamSequenceToken token = null)
            {
                this.observer = observer;

                return Task.FromResult(this as StreamSubscriptionHandle<T>);
            }

            public override bool Equals(StreamSubscriptionHandle<T> other)
            {
                return this == other;
            }
        }

        #endregion
    }
}