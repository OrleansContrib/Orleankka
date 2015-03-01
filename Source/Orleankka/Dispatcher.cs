using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Orleankka
{
    using Utility;

    class Dispatcher
    {
        static readonly string[] conventions = {"On", "Handle", "Answer", "Apply"};

        readonly Dictionary<Type, Action<object, object>> voidHandlers =
             new Dictionary<Type, Action<object, object>>();

        readonly Dictionary<Type, Func<object, object, object>> replyHandlers =
             new Dictionary<Type, Func<object, object, object>>();

        readonly Dictionary<Type, Func<object, object, Task<object>>> uniformHandlers =
             new Dictionary<Type, Func<object, object, Task<object>>>();

        readonly Type actor;        
        
        public Dispatcher(Type actor)
        {
            this.actor = actor;

            var methods = actor.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                   .Where(m =>
                          m.IsPublic &&
                          m.GetParameters().Length == 1 &&
                          !m.GetParameters()[0].IsOut &&
                          !m.GetParameters()[0].IsRetval &&
                          !m.IsGenericMethod && !m.ContainsGenericParameters &&
                          conventions.Contains(m.Name));

            foreach (var method in methods)
                Register(method);
        }

        public void Register(MethodInfo method)
        {
            RegisterUniform(method);
            RegisterNonUniform(method);
        }

        void RegisterUniform(MethodInfo method)
        {
            var message = method.GetParameters()[0].ParameterType;
            var handler = Bind.Uniform.Handler(method, actor);

            if (uniformHandlers.ContainsKey(message))
                throw new InvalidOperationException(
                    string.Format("Handler for {0} has been already defined by {1}", message, actor));

            uniformHandlers.Add(message, handler);
        }

        void RegisterNonUniform(MethodInfo method)
        {
            if (typeof(Task).IsAssignableFrom(method.ReturnType))
                return;

            if (method.ReturnType == typeof(void))
            {
                RegisterVoid(method);
                return;
            }
            
            RegisterReply(method);
        }

        void RegisterVoid(MethodInfo method)
        {
            var message = method.GetParameters()[0].ParameterType;
            var handler = Bind.NonUniform.VoidHandler(method, actor);
            voidHandlers.Add(message, handler);
        }

        void RegisterReply(MethodInfo method)
        {
            var message = method.GetParameters()[0].ParameterType;
            var handler = Bind.NonUniform.ReplyHandler(method, actor);
            replyHandlers.Add(message, handler);
        }

        public void Dispatch(Actor target, object message)
        {
            var handler = voidHandlers.Find(message.GetType());

            if (handler == null)
                throw new HandlerNotFoundException(message.GetType());

            handler(target, message);
        }

        public object DispatchResult(Actor target, object message)
        {
            var handler = replyHandlers.Find(message.GetType());

            if (handler == null)
                throw new HandlerNotFoundException(message.GetType());

            return handler(target, message);
        }

        public Task<object> DispatchAsync(Actor target, object message)
        {
            var handler = uniformHandlers.Find(message.GetType());

            if (handler == null)
                throw new HandlerNotFoundException(message.GetType());

            return handler(target, message);
        }

        [Serializable]
        internal class HandlerNotFoundException : ApplicationException
        {
            const string description = "Can't find handler for '{0}'.\r\nCheck that handler method is public, has single arg and named 'On' or 'Handle'";

            internal HandlerNotFoundException(Type message)
                : base(string.Format(description, message))
            {}

            protected HandlerNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        static class Bind
        {
            static readonly Task<object> Done = Task.FromResult((object)null);

            public static class NonUniform
            {
                public static Action<object, object> VoidHandler(MethodInfo method, Type actor)
                {
                    var compiler = typeof(Void<>)
                        .MakeGenericType(method.GetParameters()[0].ParameterType)
                        .GetMethod("Action", BindingFlags.Public | BindingFlags.Static);

                    return (Action<object, object>)compiler.Invoke(null, new object[] {method, actor});
                }

                public static Func<object, object, object> ReplyHandler(MethodInfo method, Type actor)
                {
                    var compiler = typeof(Reply<>)
                        .MakeGenericType(method.GetParameters()[0].ParameterType)
                        .GetMethod("Func", BindingFlags.Public | BindingFlags.Static)
                        .MakeGenericMethod(method.ReturnType);

                    return (Func<object, object, object>)compiler.Invoke(null, new object[] {method, actor});
                }

                static class Void<TRequest>
                {
                    public static Action<object, object> Action(MethodInfo method, Type type)
                    {
                        return method.IsStatic ? StaticAction(method) : InstanceAction(method, type);
                    }

                    static Action<object, object> StaticAction(MethodInfo method)
                    {
                        ParameterExpression request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, new Expression[] { requestConversion });
                        Action<object> action = Expression.Lambda<Action<object>>(call, request).Compile();

                        return (t, r) => action(r);
                    }

                    static Action<object, object> InstanceAction(MethodInfo method, Type type)
                    {
                        ParameterExpression target = Expression.Parameter(typeof(object));
                        ParameterExpression request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
                        Action<object, object> action = Expression.Lambda<Action<object, object>>(call, target, request).Compile();

                        return action;
                    }   
                }

                static class Reply<TRequest>
                {
                    public static Func<object, object, object> Func<TResult>(MethodInfo method, Type type)
                    {
                        return method.IsStatic ? StaticFunc<TResult>(method) : InstanceFunc<TResult>(method, type);
                    }

                    static Func<object, object, object> StaticFunc<TResult>(MethodInfo method)
                    {
                        ParameterExpression request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, new Expression[] { requestConversion });
                        var func = Expression.Lambda<Func<object, TResult>>(call, request).Compile();

                        return (t, r) => (object)func(r);
                    }

                    static Func<object, object, object> InstanceFunc<TResult>(MethodInfo method, Type type)
                    {
                        ParameterExpression target = Expression.Parameter(typeof(object));
                        ParameterExpression request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
                        var func = Expression.Lambda<Func<object, object, TResult>>(call, target, request).Compile();

                        return (t, r) => (object)func(t, r);
                    }
                }
            }

            public static class Uniform
            { 
                public static Func<object, object, Task<object>> Handler(MethodInfo method, Type actor)
                {
                    if (typeof(Task).IsAssignableFrom(method.ReturnType))
                    {
                        return method.ReturnType.GenericTypeArguments.Length != 0
                                ? Lambda(typeof(Async<>), "Func", method.ReturnType.GenericTypeArguments[0], method, actor)
                                : Lambda(typeof(Async<>), "Action", null, method, actor);
                    }

                    return method.ReturnType != typeof(void)
                            ? NonAsync.Func(method, actor)
                            : NonAsync.Action(method, actor);
                }

                static Func<object, object, Task<object>> Lambda(Type binder, string kind, Type arg, MethodInfo method, Type type)
                {
                    var compiler = binder
                        .MakeGenericType(method.GetParameters()[0].ParameterType)
                        .GetMethod(kind, BindingFlags.Public | BindingFlags.Static);

                    if (arg != null)
                        compiler = compiler.MakeGenericMethod(arg);

                    return (Func<object, object, Task<object>>)compiler.Invoke(null, new object[] { method, type });
                }

                static class Async<TRequest>
                {
                    public static Func<object, object, Task<object>> Func<TResult>(MethodInfo method, Type type)
                    {
                        return method.IsStatic ? StaticFunc<TResult>(method) : InstanceFunc<TResult>(method, type);
                    }

                    static Func<object, object, Task<object>> StaticFunc<TResult>(MethodInfo method)
                    {
                        ParameterExpression request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, new Expression[] { requestConversion });
                        var func = Expression.Lambda<Func<object, Task<TResult>>>(call, request).Compile();

                        return async (t, r) => await func(r);
                    }

                    static Func<object, object, Task<object>> InstanceFunc<TResult>(MethodInfo method, Type type)
                    {
                        ParameterExpression target = Expression.Parameter(typeof(object));
                        ParameterExpression request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
                        var func = Expression.Lambda<Func<object, object, Task<TResult>>>(call, target, request).Compile();

                        return async (t, r) => await func(t, r);
                    }

                    public static Func<object, object, Task<object>> Action(MethodInfo method, Type type)
                    {
                        return method.IsStatic ? StaticAction(method) : InstanceAction(method, type);
                    }

                    static Func<object, object, Task<object>> StaticAction(MethodInfo method)
                    {
                        ParameterExpression request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, new Expression[] { requestConversion });
                        Func<object, Task> func = Expression.Lambda<Func<object, Task>>(call, request).Compile();

                        return async (t, r) =>
                        {
                            await func(r);
                            return null;
                        };
                    }

                    static Func<object, object, Task<object>> InstanceAction(MethodInfo method, Type type)
                    {
                        ParameterExpression target = Expression.Parameter(typeof(object));
                        ParameterExpression request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
                        Func<object, object, Task> func = Expression.Lambda<Func<object, object, Task>>(call, target, request).Compile();

                        return async (t, r) =>
                        {
                            await func(t, r);
                            return null;
                        };
                    }
                }

                static class NonAsync
                {
                    public static Func<object, object, Task<object>> Func(MethodInfo method, Type type)
                    {
                        var handler = NonUniform.ReplyHandler(method, type);
                        return (t, r) => Task.FromResult(handler(t, r));
                    }

                    public static Func<object, object, Task<object>> Action(MethodInfo method, Type type)
                    {
                        var handler = NonUniform.VoidHandler(method, type);
                        return (t, r) =>
                        {
                            handler(t, r);
                            return Done;
                        };
                    }
                }
            }
        }
    }
}
