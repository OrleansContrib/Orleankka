using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka
{
    public abstract class TypedActor : Actor
    {
        static readonly Dictionary<Type, Func<object, object, Task<object>>> handlers = 
                    new Dictionary<Type, Func<object, object, Task<object>>>();

        public override Task<object> OnReceive(object message)
        {
            var handler = handlers.Find(message.GetType());

            if (handler == null)
                throw new InvalidOperationException("Ask message handler hasn't been defined for: " + message.GetType());

            return handler(this, message);
        }

        protected void On<TRequest, TResult>(Func<TRequest, TResult> handler)
        {
            handlers.Add(typeof(TRequest), BindFunc<TRequest, TResult>(handler.Method));
        }

        Func<object, object, Task<object>> BindFunc<TRequest, TResult>(MethodInfo method)
        {
            return method.IsStatic ? BindStaticFunc<TRequest, TResult>(method) : BindInstanceBoundFunc<TRequest, TResult>(method);
        }

        static Func<object, object, Task<object>> BindStaticFunc<TRequest, TResult>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] { requestConversion });
            var func = Expression.Lambda<Func<object, TResult>>(call, request).Compile();

            return (t, r) => Task.FromResult((object)func(r));
        }

        Func<object, object, Task<object>> BindInstanceBoundFunc<TRequest, TResult>(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            var targetConversion = Expression.Convert(target, GetType());
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(targetConversion, method, new Expression[] {requestConversion});
            var func = Expression.Lambda<Func<object, object, TResult>>(call, target, request).Compile();

            return (t, r) => Task.FromResult((object) func(t, r));
        }

        protected void On<TRequest, TResult>(Func<TRequest, Task<TResult>> handler)
        {
            handlers.Add(typeof(TRequest), BindAsyncAsk<TRequest, TResult>(handler.Method));
        }

        Func<object, object, Task<object>> BindAsyncAsk<TRequest, TResult>(MethodInfo method)
        {
            return method.IsStatic ? BindStaticAsyncFunc<TRequest, TResult>(method) : BindInstanceBoundAsyncFunc<TRequest, TResult>(method);
        }

        static Func<object, object, Task<object>> BindStaticAsyncFunc<TRequest, TResult>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] {requestConversion});
            var func = Expression.Lambda<Func<object, Task<TResult>>>(call, request).Compile();

            return async (t, r) => await func(r);
        }
        
        Func<object, object, Task<object>> BindInstanceBoundAsyncFunc<TRequest, TResult>(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            var targetConversion = Expression.Convert(target, GetType());
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(targetConversion, method, new Expression[] {requestConversion});
            var func = Expression.Lambda<Func<object, object, Task<TResult>>>(call, target, request).Compile();

            return async (t, r) => await func(t, r);
        }

        protected void On<TRequest>(Action<TRequest> handler)
        {
            handlers.Add(typeof(TRequest), BindAction<TRequest>(handler.Method));
        }

        Func<object, object, Task<object>> BindAction<TRequest>(MethodInfo method)
        {
            return method.IsStatic ? BindStaticAction<TRequest>(method) : BindInstanceBoundAction<TRequest>(method);
        }

        static Func<object, object, Task<object>> BindStaticAction<TRequest>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] { requestConversion });
            Action<object> action = Expression.Lambda<Action<object>>(call, request).Compile();

            return (t, r) =>
            {
                action(r);
                return Done();
            };
        }
        
        Func<object, object, Task<object>> BindInstanceBoundAction<TRequest>(MethodInfo method)
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
                return Done();
            };
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            handlers.Add(typeof(TRequest), BindAsyncAction<TRequest>(handler.Method));
        }

        Func<object, object, Task<object>> BindAsyncAction<TRequest>(MethodInfo method)
        {
            return method.IsStatic ? BindStaticAsyncAction<TRequest>(method) : BindInstanceBoundAsyncAction<TRequest>(method);
        }

        static Func<object, object, Task<object>> BindStaticAsyncAction<TRequest>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] {requestConversion});
            Func<object, Task> func = Expression.Lambda<Func<object, Task>>(call, request).Compile();

            return async (t, r) =>
            {
                await func(r);
                return Done();
            };
        }
        
        Func<object, object, Task<object>> BindInstanceBoundAsyncAction<TRequest>(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            var targetConversion = Expression.Convert(target, GetType());
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
            Func<object, object, Task> func = Expression.Lambda<Func<object, object, Task>>(call, target, request).Compile();

            return async (t, r) =>
            {
                await func(t, r);
                return Done();
            };
        }

        public static void Register(Type type)
        {
            if (!typeof(TypedActor).IsAssignableFrom(type) || type.IsAbstract)
                return;

            var proto = (TypedActor) Activator.CreateInstance(type);
            proto.Define();
        }

        public static void Reset()
        {
            handlers.Clear();
        }

        protected abstract void Define();
    }
}