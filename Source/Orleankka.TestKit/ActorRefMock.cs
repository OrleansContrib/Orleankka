using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.TestKit
{
    public class ActorRefMock : ActorRef
    {
        public readonly List<RecordedCommand> RecordedCommands = new List<RecordedCommand>();
        public readonly List<RecordedQuery> RecordedQueries = new List<RecordedQuery>();

        readonly List<IExpectation> expectations = new List<IExpectation>();

        public CommandExpectation<TCommand> ExpectTell<TCommand>(Expression<Func<TCommand, bool>> match = null)
        {
            var expectation = new CommandExpectation<TCommand>(match ?? (_ => true));
            expectations.Add(expectation);
            return expectation;
        }

        public QueryExpectation<TQuery> ExpectAsk<TQuery>(Expression<Func<TQuery, bool>> match = null)
        {
            var expectation = new QueryExpectation<TQuery>(match ?? (_ => true));
            expectations.Add(expectation);
            return expectation;
        }

        public override Task Tell(object message)
        {
            var expectation = Match(message);
            var expected = expectation != null;

            RecordedCommands.Add(
                new RecordedCommand(expected, message));

            if (expected)
                expectation.Apply();

            return TaskDone.Done;
        }

        public override Task<TResult> Ask<TResult>(object message)
        {
            var expectation = Match(message);
            var expected = expectation != null;

            RecordedQueries.Add(
                new RecordedQuery(expected, message, typeof(TResult)));

            return expected 
                       ? Task.FromResult((TResult) expectation.Apply()) 
                       : Task.FromResult(default(TResult));
        }

        IExpectation Match(object message)
        {
            return expectations.FirstOrDefault(x => x.Match(message));
        }
    }

    public class RecordedCommand
    {
        public readonly bool Expected;
        public readonly object Message;

        public RecordedCommand(bool expected, object message)
        {
            Expected = expected;
            Message = message;
        }
    }

    public class RecordedQuery
    {
        public readonly bool Expected;
        public readonly object Message;
        public readonly Type Result;

        public RecordedQuery(bool expected, object message, Type result)
        {
            Expected = expected;
            Message = message;
            Result = result;
        }
    }
}