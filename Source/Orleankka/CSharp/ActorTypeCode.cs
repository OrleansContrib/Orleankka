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

        internal static string Register(Type type)
        {
            var code = Code(type);
            codes.Add(type, code);
            return code;
        }

        internal static string Of(Type type)
        {
            var code = codes.Find(type);
            return code ?? Code(type);
        }

        static string Code(Type type)
        {
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
    }
}