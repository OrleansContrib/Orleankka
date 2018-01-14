using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core
{
    using Utility;

    class ActorTypeName
    {
        static readonly Dictionary<Type, string> map =
                    new Dictionary<Type, string>();

        internal static void Register(Type type)
        {
            var name = Name(type);
            map[type]= name;
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

            return customAttribute == null 
                ? type.FullName 
                : customAttribute.Name;
        }

        internal static Type CustomInterface(Type type)
        {
            var interfaces = type
                .GetInterfaces().Except(new[]{typeof(IActorGrain)})
                .Where(each => each.GetInterfaces().Contains(typeof(IActorGrain)))
                .Where(each => !each.IsConstructedGenericType)
                .ToArray();

            if (interfaces.Length > 1)
                throw new InvalidOperationException("Type can only implement single custom IActor interface. Type: " + type.FullName);

            return interfaces.Length == 1
                       ? interfaces[0]
                       : null;
        }
    }
}