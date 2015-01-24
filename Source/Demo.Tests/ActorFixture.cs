using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using PowerAssert;
using NUnit.Framework;
using JetBrains.Annotations;

using Orleankka.TestKit;

namespace Demo
{
    public abstract class ActorFixture
    {
        protected ActorSystemMock System;
        protected TimerServiceMock Timers;
        protected ReminderServiceMock Reminders;
        protected ActorObserverCollectionMock Observers;

        [SetUp]
        public virtual void SetUp()
        {
            System = new ActorSystemMock();
            Timers = new TimerServiceMock();
            Reminders = new ReminderServiceMock();
            Observers = new ActorObserverCollectionMock();
        }

        protected static void IsFalse([InstantHandle] Expression<Func<bool>> expression, string message = null)
        {
            var negated = Expression.Lambda<Func<bool>>(
                Expression.Not(expression.Body), 
                expression.Parameters);

            try
            {
                PAssert.IsTrue(negated);
            }
            catch (Exception e)
            {
                var expressionTrace = RemoveHeadline(e.Message);

                if (message != null)
                    Assert.Fail(message + Environment.NewLine + expressionTrace);

                Assert.Fail(expressionTrace);
            }
        }

        protected static void IsTrue([InstantHandle] Expression<Func<bool>> expression, string message = null)
        {
            try
            {
                PAssert.IsTrue(expression);
            }
            catch (Exception e)
            {
                var expressionTrace = RemoveHeadline(e.Message);

                if (message != null)
                    Assert.Fail(message + Environment.NewLine + expressionTrace);

                Assert.Fail(expressionTrace);
            }
        }

        static string RemoveHeadline(string error)
        {
            var lines = error.Split(new[] {"\n"}, StringSplitOptions.None).ToList();
            lines[0] = "";
            return string.Join("\n", lines);
        }

        protected static void Throws<TException>([InstantHandle] Func<Task> action, string message = null) where TException : Exception
        {
            Assert.Throws<TException>(async ()=> await action(), message);
        }

        protected RecordedTimer Timer(string id)
        {
            return Timers[id];
        }           
        
        protected RecordedTimer<TState> Timer<TState>(string id)
        {
            return (RecordedTimer<TState>) Timers[id];
        }        

        protected RecordedReminder Reminder(string api)
        {
            return Reminders[api];
        }
    }

    public static class ActorRefMockExtensions
    {
        public static CommandExpectation<TCommand> ExpectCommand<TCommand>(
            this ActorRefMock mock, 
            Expression<Func<TCommand, bool>> match = null) 
            where TCommand : Command
        {
            return mock.ExpectTell(match);
        }

        public static QueryExpectation<TQuery> ExpectQuery<TQuery>(
            this ActorRefMock mock, 
            Expression<Func<TQuery, bool>> match = null)
            where TQuery : Query
        {
            return mock.ExpectAsk(match);
        }

        public static void Reset(this ActorRefMock mock)
        {
            mock.RecordedCommands.Clear();
            mock.RecordedQueries.Clear();
        }

        public static IEnumerable<Command> Commands(this ActorRefMock mock)
        {
            return mock.RecordedCommands.Select(x => x.Message).Cast<Command>();
        }

        public static IEnumerable<Query> Queries(this ActorRefMock mock)
        {
            return mock.RecordedQueries.Select(x => x.Message).Cast<Query>();
        }

        public static bool DidNotReceiveAnyCommands(this ActorRefMock mock)
        {
            return !mock.Commands().Any();
        }

        public static bool DidNotReceiveAnyQueries(this ActorRefMock mock)
        {
            return !mock.Queries().Any();
        }

        public static TCommand FirstCommand<TCommand>(this ActorRefMock mock)
        {
            return mock.Commands().OfType<TCommand>().FirstOrDefault();
        }

        public static TQuery FirstQuery<TQuery>(this ActorRefMock mock)
        {
            return mock.Queries().OfType<TQuery>().FirstOrDefault();
        }
    }

    public static class ActorObserverCollectionMockExtensions
    {
        public static IEnumerable<Event> Events(this ActorObserverCollectionMock mock)
        {
            return mock.RecordedNotifications.Cast<Event>();
        }

        public static TEvent FirstEvent<TEvent>(this ActorObserverCollectionMock mock) where TEvent : Event
        {
            return mock.Events().OfType<TEvent>().FirstOrDefault();
        }
    }
}
