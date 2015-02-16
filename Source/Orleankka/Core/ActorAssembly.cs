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
            ActorEndpointDynamicFactory.Reset();
            TypedActor.Reset();
        }

        public static void Register(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                Register(assembly);
        }

        public static void Register(Assembly assembly)
        {
            var types = assembly
                .GetTypes()
                .Where(x =>
                       !x.IsAbstract
                       && typeof(Actor).IsAssignableFrom(x));

            foreach (var type in types)
            {
                ActorTypeCode.Register(type);
                ActorEndpointDynamicFactory.Register(type);
                TypedActor.Register(type);
            }
        }
    }
}
