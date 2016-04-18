using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Orleankka.Core
{
    class Reentrant
    {
        Func<object, bool> isReentrant = (message) => false;

        public Reentrant(Type actor)
        {
            if (IsTypeHasReentrantAttr(actor))
            {
                isReentrant = BuildReentrantCheckByAttr(actor);
            }
            else if (IsTypeImplReentrant(actor))
            {
                isReentrant = BuildReentrantCheckByImpl(actor);
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

        static bool IsTypeImplReentrant(Type actor)
        {
            var method = actor.GetMethod("IsReentrant", BindingFlags.Static | BindingFlags.NonPublic);
            return method != null;
        }

        static Func<object, bool> BuildReentrantCheckByAttr(Type actor)
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

        static Func<object, bool> BuildReentrantCheckByImpl(Type actor)
        {
            var method = actor.GetMethod("IsReentrant", BindingFlags.Static | BindingFlags.NonPublic);            
            var message = Expression.Parameter(typeof(object), "message");
            return Expression.Lambda<Func<object, bool>>(Expression.Call(method, message), message).Compile();
        }
    }
}
