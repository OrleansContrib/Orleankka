using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Orleankka
{
    using Utility;

    [DebuggerDisplay("_.{actor}")]
    class ActorPrototype
    {
        static readonly Dictionary<Type, ActorPrototype> cache =
                    new Dictionary<Type, ActorPrototype>();

        readonly Type actor;
        
        readonly Dictionary<Type, Func<object, object, Task<object>>> handlers = 
             new Dictionary<Type, Func<object, object, Task<object>>>();

        HashSet<Type> reentrant = new HashSet<Type>();
        Func<object, bool> isReentrant;

        bool closed;

        internal static void Register(Type actor)
        {
            var prototype = new ActorPrototype(actor);

            var instance = (Actor) Activator.CreateInstance(actor, nonPublic: true);
            instance.Prototype = prototype;
            instance.Define();

            cache.Add(actor, prototype.Close());
        }

        ActorPrototype Close()
        {
            closed = true;
            return this;
        }

        internal static void Reset()
        {
            cache.Clear();
        }

        internal static ActorPrototype Of(Type actor)
        {
            ActorPrototype prototype = cache.Find(actor);
            return prototype ?? new ActorPrototype(actor);
        }

        ActorPrototype(Type actor)
        {
            this.actor = actor;
                        
            RegisterReentrant();
            RegisterHandlers();
        }

        void RegisterReentrant()
        {
            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true);

            foreach (var attribute in attributes)
            {
                if (reentrant.Contains(attribute.Message))
                    throw new InvalidOperationException(
                        string.Format("{0} was already registered as Reentrant", attribute.Message));

                reentrant.Add(attribute.Message);
            }

            isReentrant = message => reentrant.Contains(message.GetType());
        }

        public void RegisterReentrant(Func<object, bool> predicate)
        {
            AssertClosed();

            if (reentrant == null)
                throw new InvalidOperationException(
                    "Reentrant message predicate has been set already");

            if (reentrant.Count > 0)
                throw new InvalidOperationException(
                    "Either declarative or imperative definition of reentrant messages can be used at a time");

            isReentrant = predicate;
            reentrant = null;
        }

        internal bool IsReentrant(object message)
        {
            return isReentrant(message);
        }

        void RegisterHandlers()
        {
            var methods = actor.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                               .Where(m =>
                                      m.IsPublic &&
                                      m.GetParameters().Length == 1 &&
                                      !m.GetParameters()[0].IsOut &&
                                      !m.GetParameters()[0].IsRetval &&
                                      !m.IsGenericMethod && !m.ContainsGenericParameters &&
                                      (m.Name == "On" || m.Name == "Handle"));

            foreach (var method in methods)
                RegisterHandler(method);
        }

        public void RegisterHandler(MethodInfo method)
        {
            AssertClosed();

            var message = method.GetParameters()[0].ParameterType;
            var handler = Bind.Handler(method, actor);
            
            if (handlers.ContainsKey(message))
                throw new InvalidOperationException(
                    string.Format("Handler for {0} has been already defined by {1}", message, actor));

            handlers.Add(message, handler);
        }

        void AssertClosed()
        {
            if (closed)
                throw new InvalidOperationException("Actor prototype can only be defined within Define() method");
        }

        internal Task<object> Dispatch(Actor target, object message)
        {
            var handler = handlers.Find(message.GetType());

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

            public static Func<object, object, Task<object>> Handler(MethodInfo method, Type actor)
            {
                if (typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    return method.ReturnType.GenericTypeArguments.Length != 0 
                            ? Lambda(typeof(Async<>), "Func", method.ReturnType.GenericTypeArguments[0], method, actor) 
                            : Lambda(typeof(Async<>), "Action", null, method, actor);
                }

                return method.ReturnType != typeof(void) 
                        ? Lambda(typeof(NonAsync<>), "Func", method.ReturnType, method, actor) 
                        : Lambda(typeof(NonAsync<>), "Action", null, method, actor);
            }

            static Func<object, object, Task<object>> Lambda(Type binder, string kind, Type arg, MethodInfo method, Type type)
            {
                var compiler = binder
                    .MakeGenericType(method.GetParameters()[0].ParameterType)
                    .GetMethod(kind, BindingFlags.Public | BindingFlags.Static);

                if (arg != null)
                    compiler = compiler.MakeGenericMethod(arg);
                
                return (Func<object, object, Task<object>>) compiler.Invoke(null, new object[] {method, type});
            }

            static class NonAsync<TRequest>
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
                    var func = Expression.Lambda<Func<object, TResult>>(call, request).Compile();

                    return (t, r) => Task.FromResult((object)func(r));
                }

                static Func<object, object, Task<object>> InstanceFunc<TResult>(MethodInfo method, Type type)
                {
                    ParameterExpression target = Expression.Parameter(typeof(object));
                    ParameterExpression request = Expression.Parameter(typeof(object));

                    var targetConversion = Expression.Convert(target, type);
                    var requestConversion = Expression.Convert(request, typeof(TRequest));

                    var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
                    var func = Expression.Lambda<Func<object, object, TResult>>(call, target, request).Compile();

                    return (t, r) => Task.FromResult((object)func(t, r));
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
                    Action<object> action = Expression.Lambda<Action<object>>(call, request).Compile();

                    return (t, r) =>
                    {
                        action(r);
                        return Done;
                    };
                }

                static Func<object, object, Task<object>> InstanceAction(MethodInfo method, Type type)
                {
                    ParameterExpression target = Expression.Parameter(typeof(object));
                    ParameterExpression request = Expression.Parameter(typeof(object));

                    var targetConversion = Expression.Convert(target, type);
                    var requestConversion = Expression.Convert(request, typeof(TRequest));

                    var call = Expression.Call(targetConversion, method, new Expression[] { requestConversion });
                    Action<object, object> action = Expression.Lambda<Action<object, object>>(call, target, request).Compile();

                    return (t, r) =>
                    {
                        action(t, r);
                        return Done;
                    };
                }
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
        }
    }
}