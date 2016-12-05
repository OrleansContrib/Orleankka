using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleankka.Utility;

namespace Orleankka
{
    public class Dispatcher
    {
        static readonly string[] DefaultConventions = {"On", "Handle", "Answer", "Apply"};
        static readonly Type[] DefaultRoots = {typeof(Actor), typeof(object)};

        readonly Dictionary<Type, Action<object, object>> actions =
             new Dictionary<Type, Action<object, object>>();

        readonly Dictionary<Type, Func<object, object, object>> funcs =
             new Dictionary<Type, Func<object, object, object>>();

        readonly Dictionary<Type, Func<object, object, Task<object>>> uniform =
             new Dictionary<Type, Func<object, object, Task<object>>>();

        readonly Type type;
        
        public Dispatcher(Type type, string[] conventions = null, Type[] roots = null)
        {
            this.type = type;

            var methods = GetMethods(type, 
                roots ?? DefaultRoots, 
                conventions ?? DefaultConventions);

            foreach (var method in methods)
                Register(method);
        }

        static IEnumerable<MethodInfo> GetMethods(Type type, Type[] roots, string[] conventions)
        {
            while (true)
            {
                Debug.Assert(type != null);

                if (roots.Contains(type))
                    yield break;

                const BindingFlags scope = BindingFlags.Instance |
                                           BindingFlags.Public |
                                           BindingFlags.NonPublic |
                                           BindingFlags.DeclaredOnly;

                var methods = type
                    .GetMethods(scope)
                    .Where(m =>
                            m.GetParameters().Length == 1 &&
                            !m.GetParameters()[0].IsOut &&
                            !m.GetParameters()[0].IsRetval &&
                            !m.IsGenericMethod && !m.ContainsGenericParameters &&
                            conventions.Contains(m.Name));

                foreach (var method in methods)
                    yield return method;

                type = type.BaseType;
            }
        }

        void Register(MethodInfo method)
        {
            RegisterUniform(method);
            RegisterNonUniform(method);
        }

        void RegisterUniform(MethodInfo method)
        {
            var message = method.GetParameters()[0].ParameterType;
            var handler = Bind.Uniform.Handler(method, type);

            if (uniform.ContainsKey(message))
                throw new InvalidOperationException(
                    $"Handler for {message} has been already defined by {type}");

            uniform.Add(message, handler);
        }

        void RegisterNonUniform(MethodInfo method)
        {
            if (typeof(Task).IsAssignableFrom(method.ReturnType))
                return;

            if (method.ReturnType == typeof(void))
            {
                RegisterAction(method);
                return;
            }

            RegisterFunc(method);
        }

        void RegisterAction(MethodInfo method)
        {
            var message = method.GetParameters()[0].ParameterType;
            var handler = Bind.NonUniform.ActionHandler(method, type);
            actions.Add(message, handler);
        }

        void RegisterFunc(MethodInfo method)
        {
            var message = method.GetParameters()[0].ParameterType;
            var handler = Bind.NonUniform.FuncHandler(method, type);
            funcs.Add(message, handler);
        }

        public bool CanHandle(Type message) => uniform.Find(message) != null;
        public IEnumerable<Type> Handlers   => uniform.Keys;

        public Task<object> Dispatch(object target, object message, Func<object, Task<object>> fallback = null)
        {
            var handler = uniform.Find(message.GetType());

            if (handler != null)
                return handler(target, message);

            if (fallback == null)
                throw new HandlerNotFoundException(message.GetType());

            return fallback(message);
        }

        public void DispatchAction(object target, object message, Func<object, Task<object>> fallback = null)
        {
            var handler = actions.Find(message.GetType());

            if (handler != null)
            {
                handler(target, message);
                return;
            }

            if (fallback == null)
                throw new HandlerNotFoundException(message.GetType());

            fallback(message);
        }

        public object DispatchFunc(object target, object message, Func<object, Task<object>> fallback = null)
        {
            var handler = funcs.Find(message.GetType());

            if (handler != null)
                return handler(target, message);

            if (fallback == null)
                throw new HandlerNotFoundException(message.GetType());

            return fallback(message);
        }

        [Serializable]
        internal class HandlerNotFoundException : ApplicationException
        {
            const string description = "Can't find handler for '{0}'.\r\n" +
                                       "Check that handler method has single argument and " +
                                       "named 'On', 'Handle', 'Answer' or 'Apply'";

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

            internal static class NonUniform
            {
                public static Action<object, object> ActionHandler(MethodInfo method, Type actor)
                {
                    var compiler = typeof(Void<>)
                        .MakeGenericType(method.GetParameters()[0].ParameterType)
                        .GetMethod("Action", BindingFlags.Public | BindingFlags.Static);

                    return (Action<object, object>)compiler.Invoke(null, new object[] {method, actor});
                }

                public static Func<object, object, object> FuncHandler(MethodInfo method, Type actor)
                {
                    var compiler = typeof(Result<>)
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
                        var request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, requestConversion);
                        var action = Expression.Lambda<Action<object>>(call, request).Compile();

                        return (t, r) => action(r);
                    }

                    static Action<object, object> InstanceAction(MethodInfo method, Type type)
                    {
                        Debug.Assert(method.DeclaringType != null);

                        return method.IsAssembly
                                ? InstanceAssemblyAction(method)
                                : InstancePrivateAction(method, type);
                    }

                    static Action<object, object> InstanceAssemblyAction(MethodInfo method)
                    {
                        Debug.Assert(method.DeclaringType != null);
                        
                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, method.DeclaringType);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
                        var action = Expression.Lambda<Action<object, object>>(call, target, request).Compile();

                        var obj = Activator.CreateInstance(method.DeclaringType);
                        return (t, r) => action(obj, r);
                    }

                    static Action<object, object> InstancePrivateAction(MethodInfo method, Type type)
                    {
                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
                        var action = Expression.Lambda<Action<object, object>>(call, target, request).Compile();

                        return action;
                    }
                }

                static class Result<TRequest>
                {
                    public static Func<object, object, object> Func<TResult>(MethodInfo method, Type type)
                    {
                        return method.IsStatic ? StaticFunc<TResult>(method) : InstanceFunc<TResult>(method, type);
                    }

                    static Func<object, object, object> StaticFunc<TResult>(MethodInfo method)
                    {
                        var request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, requestConversion);
                        var func = Expression.Lambda<Func<object, TResult>>(call, request).Compile();

                        return (t, r) => func(r);
                    }

                    static Func<object, object, object> InstanceFunc<TResult>(MethodInfo method, Type type)
                    {
                        Debug.Assert(method.DeclaringType != null);

                        return method.IsAssembly
                                ? InstanceAssemblyFunc<TResult>(method)
                                : InstancePrivateFunc<TResult>(method, type);
                    }

                    static Func<object, object, object> InstanceAssemblyFunc<TResult>(MethodInfo method)
                    {
                        Debug.Assert(method.DeclaringType != null);

                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, method.DeclaringType);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
                        var func = Expression.Lambda<Func<object, object, TResult>>(call, target, request).Compile();

                        var obj = Activator.CreateInstance(method.DeclaringType);
                        return (t, r) => func(obj, r);
                    }

                    static Func<object, object, object> InstancePrivateFunc<TResult>(MethodInfo method, Type type)
                    {
                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
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
                        var request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, requestConversion);
                        var func = Expression.Lambda<Func<object, Task<TResult>>>(call, request).Compile();

                        return async (t, r) => await func(r);
                    }

                    static Func<object, object, Task<object>> InstanceFunc<TResult>(MethodInfo method, Type type)
                    {
                        Debug.Assert(method.DeclaringType != null);

                        return method.IsAssembly
                                ? InstanceAssemblyFunc<TResult>(method)
                                : InstancePrivateFunc<TResult>(method, type);
                    }

                    static Func<object, object, Task<object>> InstanceAssemblyFunc<TResult>(MethodInfo method)
                    {
                        Debug.Assert(method.DeclaringType != null);

                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, method.DeclaringType);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
                        var func = Expression.Lambda<Func<object, object, Task<TResult>>>(call, target, request).Compile();

                        var obj = Activator.CreateInstance(method.DeclaringType);
                        return async (t, r) => await func(obj, r);
                    }

                    static Func<object, object, Task<object>> InstancePrivateFunc<TResult>(MethodInfo method, Type type)
                    {
                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
                        var func = Expression.Lambda<Func<object, object, Task<TResult>>>(call, target, request).Compile();

                        return async (t, r) => await func(t, r);
                    }

                    public static Func<object, object, Task<object>> Action(MethodInfo method, Type type)
                    {
                        return method.IsStatic ? StaticAction(method) : InstanceAction(method, type);
                    }

                    static Func<object, object, Task<object>> StaticAction(MethodInfo method)
                    {
                        var request = Expression.Parameter(typeof(object));
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(null, method, requestConversion);
                        var func = Expression.Lambda<Func<object, Task>>(call, request).Compile();

                        return async (t, r) =>
                        {
                            await func(r);
                            return null;
                        };
                    }

                    static Func<object, object, Task<object>> InstanceAction(MethodInfo method, Type type)
                    {
                        Debug.Assert(method.DeclaringType != null);

                        return method.IsAssembly
                                ? InstanceAssemblyAction(method)
                                : InstancePrivateAction(method, type);
                    }

                    static Func<object, object, Task<object>> InstanceAssemblyAction(MethodInfo method)
                    {
                        Debug.Assert(method.DeclaringType != null);

                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, method.DeclaringType);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
                        var func = Expression.Lambda<Func<object, object, Task>>(call, target, request).Compile();

                        var obj = Activator.CreateInstance(method.DeclaringType);
                        return async (t, r) =>
                        {
                            await func(obj, r);
                            return null;
                        };
                    }

                    static Func<object, object, Task<object>> InstancePrivateAction(MethodInfo method, Type type)
                    {
                        var target = Expression.Parameter(typeof(object));
                        var request = Expression.Parameter(typeof(object));

                        var targetConversion = Expression.Convert(target, type);
                        var requestConversion = Expression.Convert(request, typeof(TRequest));

                        var call = Expression.Call(targetConversion, method, requestConversion);
                        var func = Expression.Lambda<Func<object, object, Task>>(call, target, request).Compile();

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
                        var handler = NonUniform.FuncHandler(method, type);
                        return (t, r) => Task.FromResult(handler(t, r));
                    }

                    public static Func<object, object, Task<object>> Action(MethodInfo method, Type type)
                    {
                        var handler = NonUniform.ActionHandler(method, type);
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
