using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
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

        public TellExpectation<TMessage> ExpectNotify<TMessage>(Expression<Func<TMessage, bool>> match = null)
        {
            var expectation = new TellExpectation<TMessage>(match ?? (_ => true));
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

        public override void Notify(object message)
        {
            var expectation = Match(message);
            var expected = expectation != null;

            messages.Add(new RecordedMessage(expected, message, typeof(DoNotExpectResult)));

            if (expected)
                expectation.Apply();
        }

        IExpectation Match(object message) => expectations.FirstOrDefault(x => x.Match(message));

        public void Reset()
        {
            expectations.Clear();
            messages.Clear();
        }

        public IEnumerable<RecordedMessage> RecordedMessages => messages;
    }

    [Serializable]
    public class ActorRefMock<T> : ActorRef<T> where T : IActor
    {
        readonly ActorRefMock @ref;

        public ActorRefMock(ActorRefMock @ref)
            : base(@ref)
        {
            this.@ref = @ref;
        }

        public TellExpectation<TMessage> ExpectTell<TMessage>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T> =>
            @ref.ExpectTell(match);

        public AskExpectation<TMessage> ExpectAsk<TMessage>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T> =>
            @ref.ExpectAsk(match);

        public AskExpectation<TMessage> ExpectAsk<TMessage, TResult>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T, TResult> =>
            @ref.ExpectAsk(match);

        public TellExpectation<TMessage> ExpectNotify<TMessage>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T> =>
            @ref.ExpectNotify(match);

        public override Task Tell(ActorMessage<T> message) =>
            @ref.Tell(message);

        public override Task<TResult> Ask<TResult>(ActorMessage<T> message) =>
            @ref.Ask<TResult>(message);

        public override Task<TResult> Ask<TResult>(ActorMessage<T, TResult> message) =>
            @ref.Ask<TResult>(message);

        public override void Notify(ActorMessage<T> message) =>
            @ref.Notify(message);

        public void Reset() =>
            @ref.Reset();

        public IEnumerable<RecordedMessage> RecordedMessages =>
            @ref.RecordedMessages;
    }

    public static class ActorSystemMockExtensions
    {
        public static ActorRefMock<TActor> MockActorOf<TActor>(this ActorSystemMock system, string id) where TActor : IActor
        {
            var path = typeof(TActor).ToActorPath(id);
            return new ActorRefMock<TActor>(system.MockActorOf(path));
        }
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