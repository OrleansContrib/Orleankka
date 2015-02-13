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

        static readonly Task<object> CompletedTask = Task.FromResult((object)null);

        public override Task OnTell(object message)
        {
            var handler = handlers.Find(message.GetType());

            if (handler == null)
                throw new InvalidOperationException("Tell message handler hasn't been defined for: " + message.GetType());

            return handler(this, message);
        }

        public override Task<object> OnAsk(object message)
        {
            var handler = handlers.Find(message.GetType());

            if (handler == null)
                throw new InvalidOperationException("Ask message handler hasn't been defined for: " + message.GetType());

            return handler(this, message);
        }

        protected void On<TRequest, TResult>(Func<TRequest, TResult> handler)
        {
            handlers.Add(typeof(TRequest), BindAsk<TRequest, TResult>(handler.Method));
        }

        Func<object, object, Task<object>> BindAsk<TRequest, TResult>(MethodInfo method)
        {
            return method.IsStatic ? BindStaticAsk<TRequest, TResult>(method) : BindInstanceAsk<TRequest, TResult>(method);
        }

        static Func<object, object, Task<object>> BindStaticAsk<TRequest, TResult>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] { requestConversion });
            var func = Expression.Lambda<Func<object, TResult>>(call, request).Compile();

            return (t, r) => Task.FromResult((object)func(r));
        }

        Func<object, object, Task<object>> BindInstanceAsk<TRequest, TResult>(MethodInfo method)
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
            return method.IsStatic ? BindStaticAsyncAsk<TRequest, TResult>(method) : BindInstanceAsyncAsk<TRequest, TResult>(method);
        }

        static Func<object, object, Task<object>> BindStaticAsyncAsk<TRequest, TResult>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] {requestConversion});
            var func = Expression.Lambda<Func<object, Task<TResult>>>(call, request).Compile();

            return async (t, r) => await func(r);
        }
        
        Func<object, object, Task<object>> BindInstanceAsyncAsk<TRequest, TResult>(MethodInfo method)
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
            handlers.Add(typeof(TRequest), BindTell<TRequest>(handler.Method));
        }

        Func<object, object, Task<object>> BindTell<TRequest>(MethodInfo method)
        {
            return method.IsStatic ? BindStaticTell<TRequest>(method) : BindInstanceTell<TRequest>(method);
        }

        static Func<object, object, Task<object>> BindStaticTell<TRequest>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] { requestConversion });
            Action<object> action = Expression.Lambda<Action<object>>(call, request).Compile();

            return (t, r) =>
            {
                action(r);
                return CompletedTask;
            };
        }
        
        Func<object, object, Task<object>> BindInstanceTell<TRequest>(MethodInfo method)
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
                return CompletedTask;
            };
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            handlers.Add(typeof(TRequest), BindAsyncTell<TRequest>(handler.Method));
        }

        Func<object, object, Task<object>> BindAsyncTell<TRequest>(MethodInfo method)
        {
            return method.IsStatic ? BindStaticAsyncTell<TRequest>(method) : BindInstanceAsyncTell<TRequest>(method);
        }

        static Func<object, object, Task<object>> BindStaticAsyncTell<TRequest>(MethodInfo method)
        {
            ParameterExpression request = Expression.Parameter(typeof(object));
            var requestConversion = Expression.Convert(request, typeof(TRequest));

            var call = Expression.Call(null, method, new Expression[] {requestConversion});
            Func<object, Task> func = Expression.Lambda<Func<object, Task>>(call, request).Compile();

            return async (t, r) =>
            {
                await func(r);
                return CompletedTask;
            };
        }
        
        Func<object, object, Task<object>> BindInstanceAsyncTell<TRequest>(MethodInfo method)
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
                return CompletedTask;
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