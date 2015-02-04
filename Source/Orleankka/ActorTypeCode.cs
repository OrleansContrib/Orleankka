using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka
{
    class ActorTypeCode
    {
        static readonly Dictionary<string, Type> codeMap =
                    new Dictionary<string, Type>();

        static readonly Dictionary<Type, string> typeMap =
                    new Dictionary<Type, string>();

        readonly Type type;
        readonly string code;

        public ActorTypeCode(Type type, string code)
        {
            this.type = type;
            this.code = code;
        }

        public static void Register(Type type)
        {
            string code = CodeOf(type);

            if (codeMap.ContainsKey(code))
            {
                var existing = codeMap[code];

                if (existing != type)
                    throw new ArgumentException(
                        String.Format("The type {0} has been already registered under the code {1}. Use TypeCode attribute to provide unique code for {2}",
                                      existing.FullName, code, type.FullName));

                throw new ArgumentException(string.Format("The type {0} has been already registered", type));
            }

            codeMap.Add(code, type);
            typeMap.Add(type, code);
        }

        public static Type RegisteredType(string code)
        {
            Type type;
                
            if (!codeMap.TryGetValue(code, out type))
                throw new InvalidOperationException(
                    String.Format("Unable to map type code '{0}' to the corresponding runtime type. Make sure that you've registered the assembly containing this type", code));

            return type;
        }

        public static string RegisteredCode(Type type)
        {
            string code;

            if (!typeMap.TryGetValue(type, out code))
                throw new InvalidOperationException(
                    String.Format("Unable to map type '{0}' to the corresponding type code. Make sure that you've registered the assembly containing this type", type));

            return code;
        }

        public static void Reset()
        {
            codeMap.Clear();
            typeMap.Clear();
        }

        public static string CodeOf(Type type)
        {
            return type.Name;   // TODO: add support for ActorTypeCodeAttribute override
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActorTypeCodeAttribute : Attribute
    {}
}