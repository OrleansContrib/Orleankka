using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka.Core
{
    using Utility;

    class ActorInterfaceRegistry
    {
        readonly HashSet<string> interfaces = new HashSet<string>();
        readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();
        readonly List<ActorInterfaceMapping> mappings = new List<ActorInterfaceMapping>();

        internal Assembly[] Assemblies => assemblies.ToArray();
        internal ActorInterfaceMapping[] Mappings => mappings.ToArray();

        internal void Register(Assembly[] assemblies, Func<Assembly, IEnumerable<Type>> selector)
        {
            Requires.NotNull(assemblies, nameof(assemblies));

            if (assemblies.Length == 0)
                throw new ArgumentException("Assemblies length should be greater than 0", nameof(assemblies));

            foreach (var assembly in assemblies)
            {
                if (this.assemblies.Contains(assembly))
                    throw new ArgumentException($"Assembly {assembly.FullName} has been already registered");

                this.assemblies.Add(assembly);
            }

            foreach (var type in assemblies.SelectMany(selector))
            {
                var mapping = ActorInterfaceMapping.Of(type);

                if (interfaces.Contains(mapping.TypeName))
                {
                    var existing = mappings.Single(x => x.TypeName == mapping.TypeName);
                    throw new DuplicateActorTypeException(existing, mapping);
                }

                interfaces.Add(mapping.TypeName);
                mappings.Add(mapping);
            }
        }

        internal bool IsRegistered(string typeName) => interfaces.Contains(typeName);
    }
}