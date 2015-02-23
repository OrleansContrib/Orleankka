using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Orleankka.Core
{
    static class ActorAssembly
    {
        public static void Reset()
        {
            ActorTypeCode.Reset();
            ActorPrototype.Reset();
            ActorEndpointDynamicFactory.Reset();
        }

        public static void Register(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                Register(assembly);
        }

        public static void Register(Assembly assembly)
        {
            var actors = assembly
                .GetTypes()
                .Where(x =>
                       !x.IsAbstract
                       && typeof(Actor).IsAssignableFrom(x));

            foreach (var type in actors)
            {
                ActorTypeCode.Register(type);
                ActorPrototype.Register(type);
                ActorEndpointDynamicFactory.Register(type);
            }
        }
    }
}
