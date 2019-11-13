using System;
using System.Linq;

namespace Orleankka
{
    class ActorGrainImplementation
    {
        internal static Type InterfaceOf(Type type)
        {
            var interfaces = type
                .GetInterfaces().Except(new[] { typeof(IActorGrain), typeof(Legacy.IActor) })
                .Where(each => each.GetInterfaces().Any(x => x == typeof(IActorGrain) || x == typeof(Legacy.IActor)))
                .Where(each => !each.IsConstructedGenericType)
                .ToArray();

            if (interfaces.Length > 1)
                throw new InvalidOperationException($"Type '{type.FullName}' can only implement single custom IActorGrain interface");

            if (interfaces.Length == 0)
                throw new InvalidOperationException($"Type '{type.FullName}' does not implement custom IActorGrain interface");

            return interfaces[0];
        }

        public Type Interface { get; }
        public IActorMiddleware Middleware { get; }

        public ActorGrainImplementation(Type type, IActorMiddleware middleware)
        {
            Middleware = middleware;
            Interface = InterfaceOf(type);
        }
    }
}