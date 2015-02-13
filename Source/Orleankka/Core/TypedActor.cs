using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Core
{
    public abstract class TypedActor : Actor
    {
        static readonly Dictionary<Type, Func<object, object, Task>> handlers = 
                    new Dictionary<Type, Func<object, object, Task>>(); 
        
        object reply;

        protected void Reply(object result)
        {
            reply = result;
        }

        public override Task OnTell(object message)
        {
            var handler = handlers.Find(message.GetType());

            if (handler == null)
                throw new InvalidOperationException("Tell message handler hasn't been defined for: " + message.GetType());

            return handler(this, message);
        }

        public override async Task<object> OnAsk(object message)
        {
            var handler = handlers.Find(message.GetType());

            if (handler == null)
                throw new InvalidOperationException("Ask message handler hasn't been defined for: " + message.GetType());

            reply = null;
            await handler(this, message);

            return reply;
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            handlers.Add(typeof(TRequest), BindAsync<TRequest>(handler.Method));
        }

        Func<object, object, Task> BindAsync<TRequest>(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            var targetConversion = Expression.Convert(target, GetType());
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(targetConversion, method, new Expression[] {requestConversion});
            return Expression.Lambda<Func<object, object, Task>>(call, target, request).Compile();
        }

        protected void On<TRequest>(Action<TRequest> handler)
        {
            handlers.Add(typeof(TRequest), Bind<TRequest>(handler.Method));
        }

        Func<object, object, Task> Bind<TRequest>(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            var targetConversion = Expression.Convert(target, GetType());
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
            Action<object, object> action = Expression.Lambda<Action<object, object>>(call, target, request).Compile();

            return (t, r) =>
            {
                action(t, r);
                return TaskDone.Done;
            };
        }

        public static void Register(Type type)
        {
            if (!typeof(TypedActor).IsAssignableFrom(type) || type.IsAbstract)
                return;

            var proto = (TypedActor) Activator.CreateInstance(type);
            proto.Receive();
        }

        protected abstract void Receive();
    }
}