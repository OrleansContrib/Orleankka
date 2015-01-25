using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Orleankka
{
    static class ActorInterface
    {
        static readonly ConcurrentDictionary<Type, Type> cache =
                    new ConcurrentDictionary<Type, Type>();

        public static Type Of(Type type)
        {
            return cache.GetOrAdd(type, t =>
            {
                var found = t.GetInterfaces()
                             .Except(t.GetInterfaces().SelectMany(x => x.GetInterfaces()))
                             .Where(x => typeof(IActor).IsAssignableFrom(x))
                             .Where(x => x != typeof(IActor))
                             .ToArray();

                if (!found.Any())
                    throw new InvalidOperationException(
                        String.Format("The type '{0}' does not implement any of IActor inherited interfaces", t));

                return found[0];
            });
        }
    }
}
