using System;
using System.Collections.Generic;
using System.Reflection;

namespace Orleankka.Core
{
    class Reentrant
    {
        readonly HashSet<Type> messages = new HashSet<Type>();

        public Reentrant(Type actor)
        {
            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true);

            foreach (var attribute in attributes)
            {
                if (messages.Contains(attribute.Message))
                    throw new InvalidOperationException(
                        $"{attribute.Message} was already registered as Reentrant");

                messages.Add(attribute.Message);
            }
        }

        public bool IsReentrant(object message)
        {
            return messages.Contains(message.GetType());
        }
    }
}
