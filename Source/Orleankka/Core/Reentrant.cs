using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Orleankka.Core
{
    class Reentrant
    {
        readonly Func<object, bool> isReentrant = (message) => false;

        public Reentrant(Type actor)
        {
            if (IsTypeImplReentrancy(actor))
            {
                isReentrant = BuildReentrancyCheck(actor);
            }
            else if (IsTypeHasReentrantAttr(actor))
            {
                isReentrant = BuildReentrancyCheckByAttr(actor);
            }
        }

        public bool IsReentrant(object message)
        {
            return isReentrant(message);
        }        

        static bool IsTypeHasReentrantAttr(Type actor)
        {
            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true);
            return attributes.Any();
        }

        static bool IsTypeImplReentrancy(Type actor)
        {
            var method = actor.GetMethod("IsReentrant", BindingFlags.Static | BindingFlags.NonPublic);
            return method != null;
        }

        static Func<object, bool> BuildReentrancyCheckByAttr(Type actor)
        {
            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true);

            var messages = new HashSet<Type>();

            foreach (var attribute in attributes)
            {
                if (messages.Contains(attribute.Message))
                    throw new InvalidOperationException(
                        $"{attribute.Message} was already registered as Reentrant");

                messages.Add(attribute.Message);
            }
            return (message) => messages.Contains(message.GetType());
        }

        static Func<object, bool> BuildReentrancyCheck(Type actor)
        {
            var method = actor.GetMethod("IsReentrant", BindingFlags.Static | BindingFlags.NonPublic);            
            var message = Expression.Parameter(typeof(object), "message");
            return Expression.Lambda<Func<object, bool>>(Expression.Call(method, message), message).Compile();
        }
    }
}
