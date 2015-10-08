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

        public readonly List<RecordedItem> Pushed = 
                    new List<RecordedItem>();

        public readonly List<StreamSubscriptionMock> Subscriptions = 
                    new List<StreamSubscriptionMock>();

        public Actor Subscribed { get; private set; }
        public Actor Resumed    { get; private set; }

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

            Pushed.Add(new RecordedItem(expected, message));

            if (expected)
                expectation.Apply();

            return TaskDone.Done;
        }

        public override Task<StreamSubscription> Subscribe(Func<object, Task> callback) => Create(callback);
        public override Task<StreamSubscription> Subscribe<T>(Func<T, Task> callback) => Create(callback);
        public override Task<StreamSubscription> Subscribe(Action<object> callback) => Create(callback);
        public override Task<StreamSubscription> Subscribe<T>(Action<T> callback) => Create(callback);

        Task<StreamSubscription> Create(object callback)
        {
            var mock = new StreamSubscriptionMock(callback);
            Subscriptions.Add(mock);

            return Task.FromResult<StreamSubscription>(mock);
        }

        public override Task Subscribe(Actor actor)
        {
            Subscribed = actor;
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