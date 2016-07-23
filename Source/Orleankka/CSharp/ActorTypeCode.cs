using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;

namespace Orleankka.CSharp
{
    using Utility;

    static class ActorTypeCode
    {
        static readonly Dictionary<Type, string> codes =
                    new Dictionary<Type, string>();

        internal static void Reset() => codes.Clear();

        internal static bool IsRegistered(Type type) => 
            codes.ContainsKey(type);

        internal static string Register(Type type)
        {
            var code = Code(type);
            codes.Add(type, code);

            if (CustomInterface(type) != null)
                codes.Add(CustomInterface(type), code);

            return codes[type];
        }

        internal static string Of(Type type)
        {
            var code = codes.Find(type);
            return code ?? Code(type);
        }

        static string Code(Type type)
        {
            type = CustomInterface(type) ?? type;

            var customAttribute = type
                .GetCustomAttributes(typeof(ActorTypeCodeAttribute), false)
                .Cast<ActorTypeCodeAttribute>()
                .SingleOrDefault();

            if (customAttribute == null)
                return type.FullName;

            var code = customAttribute.Code;
            if (!SyntaxFacts.IsValidIdentifier(code))
                throw new ArgumentException($"'{code}' is not a valid identifer for type '{type}'", nameof(type));

            return code;
        }

        static Type CustomInterface(Type type)
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