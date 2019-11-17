using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    using Utility;

    class ActorCustomInterface
    {
        static readonly Dictionary<Type, string> map =
                    new Dictionary<Type, string>();

        internal static void Register(Type type)
        {
            var name = FullName(type);
            map[type]= name;
        }

        internal static string RegisteredName(Type type)
        {
            var name = map.Find(type);
            return name ?? FullName(type);
        }

        static string FullName(Type type)
        {
            type = Of(type);
            return type.FullName;
        }

        internal static Type Of(Type type)
        {
            if (type.IsInterface && type.GetInterfaces().Contains(typeof(IActor)))
                return type;

            var interfaces = type
                .GetInterfaces().Except(new[] {typeof(IActor)})
                .Where(each => each.GetInterfaces().Contains(typeof(IActor)))
                .Where(each => !each.IsConstructedGenericType)
                .ToArray();

            if (interfaces.Length > 1)
                throw new InvalidOperationException($"Type '{type.FullName}' can only implement single custom IActor interface");

            if (interfaces.Length == 0)
                throw new InvalidOperationException($"Type '{type.FullName}' does not implement custom IActor interface");

            return interfaces[0];
        }
    }
}