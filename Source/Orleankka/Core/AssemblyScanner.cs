using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Core
{
    static class AssemblyScanner
    {
        public static IEnumerable<Type> ActorInterfaces(this Assembly assembly) => assembly.GetTypes()
            .Where(type => type != typeof(IActor) && type.IsInterface && typeof(IActor).IsAssignableFrom(type));
    }
}
