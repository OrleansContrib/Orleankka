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
            Message.Reset();
            ActorTypeCode.Reset();
            ActorEndpointDynamicFactory.Reset();
        }

        public static void Register(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
                Register(assembly);
        }

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
                Message.Register(type);
            
            var actors = assembly
                .GetTypes()
                .Where(x =>
                       !x.IsAbstract
                       && typeof(Actor).IsAssignableFrom(x));

            foreach (var type in actors)
            {
                ActorTypeCode.Register(type);
                ActorEndpointDynamicFactory.Register(type);
            }
        }
    }
}
