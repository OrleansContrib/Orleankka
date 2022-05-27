using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Orleans;

namespace Orleankka
{
    class ActorGrainInterface
    {
        static IEnumerable<Type> GetImmediateInterfaces(Type type)
        {
            var interfaces = type.GetInterfaces();
            var result = new HashSet<Type>(interfaces);
            foreach (Type i in interfaces)
                result.ExceptWith(i.GetInterfaces());
            return result;
        }

        internal static Type InterfaceOf(Type type)
        {
            var x = GetImmediateInterfaces(type);

            List<Type> interfaces = new List<Type>();

            foreach (var i in x)
            {
                var implemented = i
                    .GetInterfaces()
                    .Where(each => each == typeof(IActorGrain) || each.GetInterfaces().Contains(typeof(IActorGrain)))
                    .Where(each => !each.IsConstructedGenericType)
                    .ToArray();

                if (implemented.Length > 0)
                {
                    interfaces.Add(i);
                }
            }

            if (interfaces.Count > 1)
                throw new InvalidOperationException($"Type '{type.FullName}' can only implement single custom IActorGrain interface");

            if (interfaces.Count == 0)
                throw new InvalidOperationException($"Type '{type.FullName}' does not implement custom IActorGrain interface");

            return interfaces[0];
        }

        readonly Func<IGrainFactory, string, object> factory;

        internal ActorGrainInterface(Type type)
        {
            var method = typeof(IGrainFactory).GetMethod("GetGrain", new[] {typeof(string), typeof(string)});
            var invoker = method.MakeGenericMethod(type);

            var @this = Expression.Parameter(typeof(object));
            var id = Expression.Parameter(typeof(string));
            var ns = Expression.Constant(null, typeof(string));

            var call = Expression.Call(Expression.Convert(@this, typeof(IGrainFactory)), invoker, id, ns);
            var func = Expression.Lambda<Func<object, string, object>>(call, @this, id).Compile();

            factory = func;
        }

        internal IActorGrain Proxy(string id, IGrainFactory instance) => (IActorGrain) factory(instance, id);
    }
}