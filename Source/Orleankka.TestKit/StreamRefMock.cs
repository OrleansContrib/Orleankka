using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Orleans.Streams;

namespace Orleankka.TestKit
{
    [Serializable]
    public class StreamRefMock : StreamRef
    {
        [NonSerialized] readonly MessageSerialization serialization;
        [NonSerialized] readonly List<IExpectation> expectations = new List<IExpectation>();

        [NonSerialized] readonly List<RecordedItem> items = new List<RecordedItem>();
        public IEnumerable<RecordedItem> RecordedItems => items;

        [NonSerialized] readonly List<RecordedItemBatch> batches = new List<RecordedItemBatch>();
        public IEnumerable<RecordedItemBatch> RecordedItemBatches => batches;

        [NonSerialized] readonly List<StreamSubscriptionMock> subscriptions = new List<StreamSubscriptionMock>();
        public IEnumerable<StreamSubscriptionMock> RecordedSubscriptions => subscriptions;

        internal StreamRefMock(StreamPath path, MessageSerialization serialization = null)
            : base(path)
        {
            this.serialization = serialization ?? MessageSerialization.Default;
        }

        public PublishExpectation<(TItem, StreamSequenceToken)> ExpectPublish<TItem>(Expression<Func<(TItem, StreamSequenceToken), bool>> match = null)
        {
            var expectation = new PublishExpectation<(TItem, StreamSequenceToken)>(match ?? (_ => true));
            expectations.Add(expectation);
            return expectation;
        }

        public PublishExpectation<(IEnumerable<TItem>, StreamSequenceToken)> ExpectPublishBatch<TItem>(Expression<Func<(IEnumerable<TItem>, StreamSequenceToken), bool>> match = null)
        {
            var expectation = new PublishExpectation<(IEnumerable<TItem>, StreamSequenceToken)>(match ?? (_ => true));
            expectations.Add(expectation);
            return expectation;
        }

        public override Task Publish(object item, StreamSequenceToken token = null)
        {
            item = Roundtrip(item);

            var expectation = Match((item, token));
            var expected = expectation != null;

            items.Add(new RecordedItem(expected, item, token));

            if (expected)
                expectation.Apply();

            return Task.CompletedTask;
        }

        public override Task Publish(IEnumerable<object> batch, StreamSequenceToken token = null)
        {
            batch = (IEnumerable<object>) Roundtrip(batch);

            var expectation = Match((batch, token));
            var expected = expectation != null;

            batches.Add(new RecordedItemBatch(expected, items, token));

            if (expected)
                expectation.Apply();

            return Task.CompletedTask;
        }

        public override Task<IList<StreamSubscription>> Subscriptions() => 
            Task.FromResult<IList<StreamSubscription>>(subscriptions.ToList<StreamSubscription>());

        public override Task<StreamSubscription> Subscribe(Func<object, Task> callback, StreamFilter filter = null) => Create(callback, filter);
        public override Task<StreamSubscription> Subscribe<T>(Func<T, Task> callback, StreamFilter filter = null) => Create(callback, filter);
        public override Task<StreamSubscription> Subscribe(Action<object> callback, StreamFilter filter = null) => Create(callback, filter);
        public override Task<StreamSubscription> Subscribe<T>(Action<T> callback, StreamFilter filter = null) => Create(callback, filter);

        Task<StreamSubscription> Create(object callback, StreamFilter filter)
        {
            var mock = new StreamSubscriptionMock(callback, filter);
            subscriptions.Add(mock);

            return Task.FromResult<StreamSubscription>(mock);
        }

        IExpectation Match(object message) => expectations.FirstOrDefault(x => x.Match(message));
        object Roundtrip(object message) => serialization.Roundtrip(message);

        public void Reset()
        {
            items.Clear();
            batches.Clear();
            subscriptions.Clear();
            expectations.Clear();
        }
    }

    public class RecordedItem
    {
        public readonly bool Expected;
        public readonly object Item;
        public readonly StreamSequenceToken Token;

        public RecordedItem(bool expected, object item, StreamSequenceToken token)
        {
            Expected = expected;
            Item = item;
            Token = token;
        }
    }

    public class RecordedItemBatch
    {
        public readonly bool Expected;
        public readonly IEnumerable<object> Items;
        public readonly StreamSequenceToken Token;

        public RecordedItemBatch(bool expected, IEnumerable<object> items, StreamSequenceToken token)
        {
            Expected = expected;
            Items = items;
            Token = token;
        }
    }
}