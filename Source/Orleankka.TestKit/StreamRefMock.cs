using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Orleankka.TestKit
{
    [Serializable]
    public class StreamRefMock<TItem> : StreamRef<TItem>
    {
        [NonSerialized] readonly MessageSerialization serialization;
        [NonSerialized] readonly List<IExpectation> expectations = new List<IExpectation>();

        [NonSerialized] readonly List<RecordedPublishMessage> published = new List<RecordedPublishMessage>();
        public IEnumerable<RecordedPublishMessage> RecordedMessages => published;

        [NonSerialized] readonly List<StreamSubscriptionMock<TItem>> subscribes = new List<StreamSubscriptionMock<TItem>>();
        public IEnumerable<StreamSubscriptionMock<TItem>> RecordedSubscribes => subscribes;

        [NonSerialized] readonly List<StreamSubscriptionMock<TItem>> resumes = new List<StreamSubscriptionMock<TItem>>();
        public IEnumerable<StreamSubscriptionMock<TItem>> RecordedResumes => resumes;

        internal StreamRefMock(StreamPath path, MessageSerialization serialization = null)
            : base(path)
        {
            this.serialization = serialization ?? MessageSerialization.Default;
        }

        public PublishExpectation<TMessage> ExpectPublish<TMessage>(Expression<Func<TMessage, bool>> match = null) 
            where TMessage : PublishMessage
        {
            var expectation = new PublishExpectation<TMessage>(match);
            expectations.Add(expectation);
            return expectation;
        }

        public SubscribeExpectation<TOptions> ExpectSubscribe<TOptions>(Expression<Func<TOptions, bool>> match = null) 
            where TOptions : SubscribeOptions
        {
            var expectation = new SubscribeExpectation<TOptions>(match);
            expectations.Add(expectation);
            return expectation;
        }

        public ResumeExpectation<TOptions> ResumeSubscribe<TOptions>(Expression<Func<TOptions, bool>> match = null) 
            where TOptions : ResumeOptions
        {
            var expectation = new ResumeExpectation<TOptions>(match);
            expectations.Add(expectation);
            return expectation;
        }

        public override Task Publish<TMessage>(TMessage message)
        {
            message = Roundtrip(message);

            var expectation = Match(message);
            var expected = expectation != null;

            published.Add(new RecordedPublishMessage(expected, message));

            if (expected)
                expectation.Apply();

            return Task.CompletedTask;
        }

        public override Task<StreamSubscription<TItem>> Subscribe<TOptions>(Func<StreamMessage, Task> callback, TOptions options)
        {
            var expectation = Match(options);
            var expected = expectation != null;

            var mock = new StreamSubscriptionMock<TItem>(this, callback, options);
            subscribes.Add(mock);

            if (expected)
                expectation.Apply();

            return Task.FromResult<StreamSubscription<TItem>>(mock);
        }

        internal Task<StreamSubscription<TItem>> Resume<TOptions>(Func<StreamMessage, Task> callback, TOptions options) 
            where TOptions : ResumeOptions
        {
            var expectation = Match(options);
            var expected = expectation != null;

            var mock = new StreamSubscriptionMock<TItem>(this, callback, null, options);
            resumes.Add(mock);

            if (expected)
                expectation.Apply();

            return Task.FromResult<StreamSubscription<TItem>>(mock);
        }

        public override Task<IList<StreamSubscription<TItem>>> Subscriptions()
        {
            var result = subscribes.ToList<StreamSubscription<TItem>>();
            return Task.FromResult<IList<StreamSubscription<TItem>>>(result);
        }

        IExpectation Match(object message) => expectations.FirstOrDefault(x => x.Match(message));
        T Roundtrip<T>(T message) => (T) serialization.Roundtrip(message);

        public void Reset()
        {
            published.Clear();
            subscribes.Clear();
            expectations.Clear();
        }
    }

    public class RecordedPublishMessage
    {
        public readonly bool Expected;
        public readonly PublishMessage Message;

        public RecordedPublishMessage(bool expected, PublishMessage message)
        {
            Message = message;
            Expected = expected;
        }
    }
}