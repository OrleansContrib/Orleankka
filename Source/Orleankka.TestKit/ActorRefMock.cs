using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    using Core;

    [Serializable]
    public class ActorRefMock : ActorRef
    {
        [NonSerialized] readonly List<IExpectation> expectations = new List<IExpectation>();
        [NonSerialized] readonly List<RecordedMessage> messages = new List<RecordedMessage>();

        public ActorRefMock(ActorPath path)
            : base(path)
        {}

        public TellExpectation<TMessage> ExpectTell<TMessage>(Expression<Func<TMessage, bool>> match = null)
        {
            var expectation = new TellExpectation<TMessage>(match ?? (_ => true));
            expectations.Add(expectation);
            return expectation;
        }

        public AskExpectation<TMessage> ExpectAsk<TMessage>(Expression<Func<TMessage, bool>> match = null)
        {
            var expectation = new AskExpectation<TMessage>(match ?? (_ => true));
            expectations.Add(expectation);
            return expectation;
        }

        public override Task Tell(object message)
        {
            var expectation = Match(message);
            var expected = expectation != null;

            messages.Add(new RecordedMessage(expected, message, typeof(DoNotExpectResult)));

            if (expected)
                expectation.Apply();

            return TaskDone.Done;
        }

        public override Task<TResult> Ask<TResult>(object message)
        {
            var expectation = Match(message);
            var expected = expectation != null;

            messages.Add(new RecordedMessage(expected, message, typeof(TResult)));

            return expected 
                       ? Task.FromResult((TResult) expectation.Apply()) 
                       : Task.FromResult(default(TResult));
        }

        IExpectation Match(object message) => expectations.FirstOrDefault(x => x.Match(message));

        public new void Reset()
        {
            expectations.Clear();
            messages.Clear();
        }

        public IEnumerable<RecordedMessage> RecordedMessages => messages;
    }

    public class RecordedMessage
    {
        public readonly bool Expected;
        public readonly object Message;
        public readonly Type Result;

        public RecordedMessage(bool expected, object message, Type result)
        {
            Expected = expected;
            Message = message;
            Result = result;
        }
    }

    public class DoNotExpectResult
    {}
}