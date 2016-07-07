using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using PowerAssert;
using NUnit.Framework;
using JetBrains.Annotations;

using Orleankka;
using Orleankka.TestKit;

namespace Demo
{
    public abstract class ActorFixture
    {
        protected ActorContextMock Context;

        [SetUp]
        public virtual void SetUp()
        {
            Context = new ActorContextMock(Path());
        }

        protected abstract ActorPath Path();

        protected ActorSystemMock System => Context.System;
        protected TimerServiceMock Timers => Context.Timers;
        protected ReminderServiceMock Reminders => Context.Reminders;
        protected ActivationServiceMock Activation => Context.Activation;

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

            if (lines.Count == 1)
                return error;

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
