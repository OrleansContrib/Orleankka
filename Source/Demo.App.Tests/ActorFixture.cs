using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using PowerAssert;
using NUnit.Framework;
using JetBrains.Annotations;

using Orleankka.Meta;
using Orleankka.TestKit;

namespace Demo
{
    public abstract class ActorFixture
    {
        protected ActorSystemMock System;
        protected TimerServiceMock Timers;
        protected ReminderServiceMock Reminders;
        protected ObserverCollectionMock Observers;

        [SetUp]
        public virtual void SetUp()
        {
            System = new ActorSystemMock();
            Timers = new TimerServiceMock();
            Reminders = new ReminderServiceMock();
            Observers = new ObserverCollectionMock();
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

        protected RecordedReminder Reminder(string id)
        {
            return Reminders[id];
        }
    }
}
