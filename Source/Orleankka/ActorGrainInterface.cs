using System;
using System.Linq;
using System.Linq.Expressions;

using Orleans;

namespace Orleankka
{
    class ActorGrainInterface
    {
        internal static Type InterfaceOf(Type type)
        {
            var interfaces = type
                .GetInterfaces().Except(new[] {typeof(IActorGrain)})
                .Where(each => each.GetInterfaces().Contains(typeof(IActorGrain)))
                .Where(each => !each.IsConstructedGenericType)
                .ToArray();

            if (interfaces.Length > 1)
                throw new InvalidOperationException($"Type '{type.FullName}' can only implement single custom IActorGrain interface");

            if (interfaces.Length == 0)
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