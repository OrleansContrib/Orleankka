using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Core
{
    static class AssemblyScanner
    {
        public static IEnumerable<Type> ActorTypes(this Assembly assembly) => assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(ActorGrain).IsAssignableFrom(type));
    }
}
