using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using PowerAssert;
using NUnit.Framework;
using JetBrains.Annotations;

using Orleankka.TestKit;

namespace Demo
{
    using Microsoft.Extensions.Hosting;

    using Orleans.Hosting;

    public abstract class ActorFixture
    {
        static readonly IHost host;

        static ActorFixture()
        {
            host = new HostBuilder()
                .UseOrleans(c => c.UseLocalhostClustering())
                .Build();

            host.StartAsync().Wait();
        }

        protected ActorRuntimeMock Runtime;

        [SetUp]
        public virtual void SetUp()
        {
            Runtime = new ActorRuntimeMock(new MessageSerialization(host.Services));
        }

        protected ActorSystemMock System => Runtime.System;
        protected TimerServiceMock Timers => Runtime.Timers;
        protected ReminderServiceMock Reminders => Runtime.Reminders;
        protected ActivationServiceMock Activation => Runtime.Activation;

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
            Assert.ThrowsAsync<TException>(async ()=> await action(), message);
        }

        protected RecordedTimer Timer(string id)
        {
            return Timers[id];
        }           
        
        protected RecordedCallbackTimer<TState> Timer<TState>(string id)
        {
            return (RecordedCallbackTimer<TState>) Timers[id];
        }

        protected RecordedReminder Reminder(string id)
        {
            return Reminders[id];
        }
    }
}
