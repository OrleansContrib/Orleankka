using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    using Core;

    public class StreamRefMock : StreamRef
    {
        static StreamRefMock()
        {
            OrleansSerialization.Hack();
        }

        readonly IMessageSerializer serializer;
        readonly List<IExpectation> expectations = new List<IExpectation>();

        readonly List<RecordedItem> items = new List<RecordedItem>();
        public IEnumerable<RecordedItem> RecordedItems => items;  

        readonly List<StreamSubscriptionMock> subscriptions = new List<StreamSubscriptionMock>();
        public IEnumerable<StreamSubscriptionMock> RecordedSubscriptions => subscriptions;

        public StreamFilter Filter { get; private set; }
        public Actor Subscribed    { get; private set; }
        public Actor Resumed       { get; private set; }

        public StreamRefMock(StreamPath path, IMessageSerializer serializer = null)
            : base(path)
        {
            this.serializer = serializer;
        }

        public PushExpectation<TItem> Expect<TItem>(Expression<Func<TItem, bool>> match = null)
        {
            var expectation = new PushExpectation<TItem>(match ?? (_ => true));
            expectations.Add(expectation);
            return expectation;
        }

        public override Task Push(object message)
        {
            message = Reserialize(message);

            var expectation = Match(message);
            var expected = expectation != null;

            items.Add(new RecordedItem(expected, message));

            if (expected)
                expectation.Apply();

            return TaskDone.Done;
        }

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

        public override Task Subscribe(Actor actor, StreamFilter filter = null)
        {
            Subscribed = actor;
            Filter = filter;
            return TaskDone.Done;
        }

        public override Task Unsubscribe(Actor actor)
        {
            if (Subscribed == actor)
                Subscribed = null; 

            return TaskDone.Done;
        }

        public override Task Resume(Actor actor)
        {
            Resumed = actor;
            return TaskDone.Done;
        }

        IExpectation Match(object message) => expectations.FirstOrDefault(x => x.Match(message));
        object Reserialize(object message) => OrleansSerialization.Reserialize(serializer, message);

        public new void Reset()
        {
            items.Clear();
            subscriptions.Clear();
            expectations.Clear();
        }
    }

    public class RecordedItem
    {
        public readonly bool Expected;
        public readonly object Item;

        public RecordedItem(bool expected, object item)
        {
            Expected = expected;
            Item = item;
        }
    }
}