using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Streams;

namespace Orleankka.TestKit
{
    public class StreamRefStub : StreamRef, IStreamIdentity
    {
        private static readonly IDictionary<StreamRefStub, IList<StreamSubscriptionHandle<object>>> subscriptionHandles =
            new ConcurrentDictionary<StreamRefStub, IList<StreamSubscriptionHandle<object>>>();

        public StreamRefStub(StreamPath path)
            : base(path)
        {
            if (!subscriptionHandles.ContainsKey(this))
                subscriptionHandles.Add(this, new List<StreamSubscriptionHandle<object>>());
        }

        IEnumerable<StubStreamSubscriptionHandle<object>> StubStreamSubscriptionHandles
        {
            get { return subscriptionHandles[this].OfType<StubStreamSubscriptionHandle<object>>(); }
        }

        IList<StreamSubscriptionHandle<object>> StreamSubscriptionHandles
        {
            get { return subscriptionHandles[this]; }
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

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + Namespace.GetHashCode();
                hash = (hash * 7) + Guid.GetHashCode();

                return hash;
            }
        }

        public override async Task OnCompletedAsync()
        {
            foreach (var streamSubscriptionHandle in StubStreamSubscriptionHandles)
                await streamSubscriptionHandle.Observer.OnCompletedAsync();
        }

        public override async Task OnNextBatchAsync(IEnumerable<object> batch, StreamSequenceToken token = null)
        {
            foreach (var streamSubscriptionHandle in StubStreamSubscriptionHandles)
                await streamSubscriptionHandle.Observer.OnNextAsync(batch, token);
        }

        public override async Task OnErrorAsync(Exception ex)
        {
            foreach (var streamSubscriptionHandle in StubStreamSubscriptionHandles)
                await streamSubscriptionHandle.Observer.OnErrorAsync(ex);
        }

        public override async Task OnNextAsync(object item, StreamSequenceToken token = null)
        {
            foreach (var streamSubscriptionHandle in StubStreamSubscriptionHandles)
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
            return Task.FromResult(StreamSubscriptionHandles);
        }

        public override Task<StreamSubscriptionHandle<object>> SubscribeAsync(IAsyncObserver<object> observer)
        {
            var streamSubscriptionHandle = new StubStreamSubscriptionHandle<object>(Guid.NewGuid(), this, observer);

            StreamSubscriptionHandles.Add(streamSubscriptionHandle);

            return Task.FromResult(streamSubscriptionHandle as StreamSubscriptionHandle<object>);
        }

        void Unsubscribe(Guid handleId)
        {
            var streamSubscriptionHandle = StubStreamSubscriptionHandles.First(a => a.HandleId == handleId);

            StreamSubscriptionHandles.Remove(streamSubscriptionHandle);
        }

        bool Equals(StreamRefStub other)
        {
            return Namespace == other.Namespace && Guid == other.Guid;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(StreamRefStub))
                return false;
            return Equals((StreamRefStub) obj);
        }

        public static bool operator ==(StreamRefStub left, StreamRefStub right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StreamRefStub left, StreamRefStub right)
        {
            return !Equals(left, right);
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