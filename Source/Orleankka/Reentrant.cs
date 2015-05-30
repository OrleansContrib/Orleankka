using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Utility;

    class Reentrant
    {
        readonly HashSet<Type> messages = new HashSet<Type>();
        Func<object, bool> evaluator = message => false;

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
        }

        public void Register(Func<object, bool> evaluator)
        {
            this.evaluator = evaluator;
        }

        public bool IsReentrant(object message)
        {
            return messages.Contains(message.GetType()) || evaluator(message);
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
