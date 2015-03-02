using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Utility;

    class Reentrant
    {
        HashSet<Type> messages = new HashSet<Type>();
        Func<object, bool> evaluator;

        public Reentrant(Type actor)
        {
            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true);

            foreach (var attribute in attributes)
            {
                if (messages.Contains(attribute.Message))
                    throw new InvalidOperationException(
                        string.Format("{0} was already registered as Reentrant", attribute.Message));

                messages.Add(attribute.Message);
            }

            evaluator = message => messages.Contains(message.GetType());
        }

        public void Register(Func<object, bool> evaluator)
        {
            if (messages == null)
                throw new InvalidOperationException(
                    "Reentrant message evaluator has been already set");

            if (messages.Count > 0)
                throw new InvalidOperationException(
                    "Either declarative or imperative definition of reentrant messages can be used at a time");

            this.evaluator = evaluator;
            messages = null;
        }

        public bool IsReentrant(object message)
        {
            return evaluator(message);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ReentrantAttribute : Attribute
    {
        internal readonly Type Message;

        public ReentrantAttribute(Type message)
        {
            Requires.NotNull(message, "message");
            Message = message;
        }
    }
}
