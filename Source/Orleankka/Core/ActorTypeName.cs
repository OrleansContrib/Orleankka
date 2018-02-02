using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    using Utility;

    class ActorTypeName
    {
        static readonly Dictionary<Type, string> names =
                    new Dictionary<Type, string>();

        static readonly Dictionary<string, Type> types =
                    new Dictionary<string, Type>();

        internal static void Register(Type type)
        {
            var name = Name(type);
            names[type]= name;
            types[name]= type;
        }

        internal static string Of(Type type)
        {
            var name = names.Find(type);
            return name ?? Name(type);
        }

        internal static Type Of(string name)
        {
            var type = types.Find(name);
            return type ?? Type(name);
        }

        static string Name(Type type)
        {
            if (type.IsInterface && typeof(IActorGrain).IsAssignableFrom(type))
                return type.FullName;

            var interfaces = type
                .GetInterfaces().Except(new[]{typeof(IActorGrain)})
                .Where(each => each.GetInterfaces().Contains(typeof(IActorGrain)))
                .Where(each => !each.IsConstructedGenericType)
                .ToArray();

            if (interfaces.Length > 1)
                throw new InvalidOperationException($"Type '{type.FullName}' can only implement single custom IActorGrain interface");

            if (interfaces.Length == 0)
                throw new InvalidOperationException($"Type '{type.FullName}' does not implement custom IActorGrain interface");

            return interfaces[0].FullName;
        }

        static Type Type(string name) => System.Type.GetType(name);
    }
}