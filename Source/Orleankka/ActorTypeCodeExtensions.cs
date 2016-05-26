using System;
using System.Linq;

namespace Orleankka
{
    internal static class ActorTypeCode
    {
        internal static string Of(Type type)
        {
            var customAttribute = type
                .GetCustomAttributes(typeof(ActorTypeCodeAttribute), false)
                .Cast<ActorTypeCodeAttribute>()
                .SingleOrDefault();

            return customAttribute != null
                    ? customAttribute.Code
                    : type.FullName;
        }
    }
}