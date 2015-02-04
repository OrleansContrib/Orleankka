using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.Core
{
    using Hardcore;

    static class ActorEndpointFactory
    {
        readonly static Dictionary<Type, ActorEndpointInvoker> invokers =
                    new Dictionary<Type, ActorEndpointInvoker>();

        public static ActorEndpointInvoker Invoker(Type type)
        {
            return invokers[type];
        }

        public static void Register(Type type)
        {
            var blend = Blend.From(type);
            Recipe.AssertValid(blend, type);
            invokers.Add(type, Bind(blend.ToString()));
        }

        static ActorEndpointInvoker Bind(string name)
        {
            var factory = Type.GetType("Orleankka.Core.Hardcore." + name + ".ActorEndpointFactory, Orleankka.Core");
            return new ActorEndpointInvoker(factory);
        }

        public static void Reset()
        {
            invokers.Clear();
        }
    }

    class ActorEndpointInvoker
    {
        public readonly Func<string, object> GetProxy;
        public readonly Func<object, RequestEnvelope, Task> ReceiveTell;
        public readonly Func<object, RequestEnvelope, Task<ResponseEnvelope>> ReceiveAsk;

        internal ActorEndpointInvoker(Type factory)
        {
            GetProxy = BindGetProxy(factory);
            ReceiveTell = BindReceiveTell(factory);
            ReceiveAsk = BindReceiveAsk(factory);
        }

        static Func<string, object> BindGetProxy(Type factory)
        {
            var parameter = Expression.Parameter(typeof(string), "primaryKey");
            var call = Expression.Call(GetGrainMethod(factory), new Expression[] {parameter});
            return Expression.Lambda<Func<string, object>>(call, parameter).Compile();
        }

        static MethodInfo GetGrainMethod(Type factory)
        {
            return factory.GetMethod("GetGrain", 
                    BindingFlags.Public | BindingFlags.Static, 
                    null, new[] {typeof(string)}, null);
        }

        static Func<object, RequestEnvelope, Task> BindReceiveTell(Type factory)
        {
            var @interface = GetGrainMethod(factory).ReturnType;

            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(RequestEnvelope));

            var conversion = Expression.Convert(target, @interface);
            var call = Expression.Call(conversion, GetTellMethod(@interface), new Expression[] { request });

            return Expression.Lambda<Func<object, RequestEnvelope, Task>>(call, target, request).Compile();
        }

        static MethodInfo GetTellMethod(Type @interface)
        {
            return @interface.GetMethod("ReceiveTell");
        }

        static Func<object, RequestEnvelope, Task<ResponseEnvelope>> BindReceiveAsk(Type factory)
        {
            var @interface = GetGrainMethod(factory).ReturnType;

            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(RequestEnvelope));

            var conversion = Expression.Convert(target, @interface);
            var call = Expression.Call(conversion, GetAskMethod(@interface), new Expression[] {request});

            return Expression.Lambda<Func<object, RequestEnvelope, Task<ResponseEnvelope>>>(call, target, request).Compile();
        }

        static MethodInfo GetAskMethod(Type @interface)
        {
            return @interface.GetMethod("ReceiveAsk");
        }
    }
}