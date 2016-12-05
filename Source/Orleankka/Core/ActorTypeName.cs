using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;

namespace Orleankka.Core
{
    using Utility;

    struct ActorTypeName
    {
        static readonly Dictionary<Type, string> map =
                    new Dictionary<Type, string>();

        internal static void Reset() => map.Clear();

        internal static void Register(Type type)
        {
            var name = Name(type);
            map.Add(type, name);
        }

        internal static string Of(Type type)
        {
            var name = map.Find(type);
            return name ?? Name(type);
        }

        static string Name(Type type)
        {
            type = CustomInterface(type) ?? type;

            var customAttribute = type
                .GetCustomAttributes(typeof(ActorTypeAttribute), false)
                .Cast<ActorTypeAttribute>()
                .SingleOrDefault();

            if (customAttribute == null)
                return type.FullName;

            var name = customAttribute.Name;
            if (!SyntaxFacts.IsValidIdentifier(name))
                throw new ArgumentException($"'{name}' is not a valid identifer for type '{type}'", nameof(type));

            return name;
        }

        internal static Type CustomInterface(Type type)
        {
            var interfaces = type
                .GetInterfaces().Except(new[] {typeof(IActor)})
                .Where(each => each.GetInterfaces().Contains(typeof(IActor)))
                .ToArray();

            if (interfaces.Length > 1)
                throw new InvalidOperationException("Type can only implement single custom IActor interface. Type: " + type.FullName);

            return interfaces.Length == 1
                       ? interfaces[0]
                       : null;
        }
    }
}