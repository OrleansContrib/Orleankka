using System;
using System.Linq.Expressions;

namespace Orleankka.TestKit
{
    public interface IExpectation
    {
        bool Match(object message);
        object Apply();
    }

    sealed class Expectation<TMessage> : IExpectation
    {
        readonly object result;
        readonly Exception exception;

        readonly Expression<Func<TMessage, bool>> expression;
        int times;

        public Expectation(Expression<Func<TMessage, bool>> expression, object result, Exception exception, int times)
        {
            this.expression = expression;
            this.result = result;
            this.exception = exception;
            this.times = times;
        }

        public bool Match(object message)
        {
            var match = MessageMatches(message) && ExpressionMatches(message);

            if (!match)
                return false;

            return times > 0;
        }

        static bool MessageMatches(object message)
        {
            return message.GetType() == typeof(TMessage);
        }

        bool ExpressionMatches(object query)
        {
            if (expression == null)
                return true;

            var applied = expression.Body.ApplyParameter(query);
            var lambda = Expression.Lambda<Func<bool>>(applied);

            return lambda.Compile()();
        }

        public object Apply()
        {
            times--;

            if (exception != null)
                throw exception;

            return result;
        }
    }

    public interface IRepeatExpectation : IExpectation
    {
        IExpectation Times(int times);
    }

    public sealed class TellExpectation<TMessage> : IRepeatExpectation
    {
        Expectation<TMessage> expectation;
        readonly Expression<Func<TMessage, bool>> expression;
        Exception exception;

        internal TellExpectation(Expression<Func<TMessage, bool>> expression)
        {
            this.expression = expression;
        }

        public IRepeatExpectation Throw(Exception exception)
        {
            this.exception = exception;
            expectation = Create();
            return this;
        }

        IExpectation IRepeatExpectation.Times(int times)
        {
            expectation = Create(times);
            return this;
        }

        Expectation<TMessage> Create(int times = int.MaxValue)
        {
            return new Expectation<TMessage>(expression, null, exception, times);
        }

        bool IExpectation.Match(object message)
        {
            if (expectation == null)
                throw IncompleteExpectationException();

            return expectation.Match(message);
        }

        object IExpectation.Apply()
        {
            if (expectation == null)
                throw IncompleteExpectationException();

            return expectation.Apply();
        }

        static InvalidOperationException IncompleteExpectationException() => new InvalidOperationException(
            "Expectation is incomplete. Configure the expectation by calling either 'Throw(') or 'Times()' methods");
    }

    public sealed class AskExpectation<TMessage> : IRepeatExpectation
    {
        Expectation<TMessage> expectation;
        readonly Expression<Func<TMessage, bool>> expression;
        Exception exception;
        object result;

        internal AskExpectation(Expression<Func<TMessage, bool>> expression)
        {
            this.expression = expression;
        }

        public IRepeatExpectation Throw(Exception exception)
        {
            this.exception = exception;
            expectation = Create();
            return this;
        }

        public IRepeatExpectation Return<TResult>(TResult result)
        {
            this.result = result;
            expectation = Create();
            return this;
        }
        
        IExpectation IRepeatExpectation.Times(int times)
        {
            expectation = Create(times);
            return this;
        }

        Expectation<TMessage> Create(int times = int.MaxValue)
        {
            return new Expectation<TMessage>(expression, result, exception, times);
        }

        bool IExpectation.Match(object message)
        {
            if (expectation == null)
                throw IncompleteExpectationException();

            return expectation.Match(message);
        }

        object IExpectation.Apply()
        {
            if (expectation == null)
                throw IncompleteExpectationException();

            return expectation.Apply();
        }

        static InvalidOperationException IncompleteExpectationException() => new InvalidOperationException(
            "Expectation is incomplete. Configure the expectation by calling either 'Throw()', 'Times()' or `Return` methods");
    }

    public sealed class PublishExpectation<TItem> : IRepeatExpectation
    {
        Expectation<TItem> expectation;
        readonly Expression<Func<TItem, bool>> expression;
        Exception exception;

        internal PublishExpectation(Expression<Func<TItem, bool>> expression)
        {
            this.expression = expression;
        }

        public IRepeatExpectation Throw(Exception exception)
        {
            this.exception = exception;
            expectation = Create();
            return this;
        }

        IExpectation IRepeatExpectation.Times(int times)
        {
            expectation = Create(times);
            return this;
        }

        Expectation<TItem> Create(int times = int.MaxValue)
        {
            return new Expectation<TItem>(expression, null, exception, times);
        }

        bool IExpectation.Match(object message)
        {
            if (expectation == null)
                throw IncompleteExpectationException();

            return expectation.Match(message);
        }

        object IExpectation.Apply()
        {
            if (expectation == null)
                throw IncompleteExpectationException();

            return expectation.Apply();
        }

        static InvalidOperationException IncompleteExpectationException() => new InvalidOperationException(
            "Expectation is incomplete. Configure the expectation by calling either 'Throw()' or 'Times()' methods");
    }

    public static class ExpectationExtensions
    {
        public static IExpectation Once(this IRepeatExpectation repetition)
        {
            return repetition.Times(1);
        }
    }

    public sealed class ExpectationException : Exception
    {
        public ExpectationException(string message, params object[] args)
            : base(string.Format(message, args))
        {}
    }
}
