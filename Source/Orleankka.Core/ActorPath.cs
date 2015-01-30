using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    [DebuggerDisplay("{TypeCode}::{Id}")]
    public struct ActorPath : IEquatable<ActorPath>
    {
        public static readonly ActorPath Empty = new ActorPath();
        public static readonly string[] Separator = {"::"};
        
        public readonly string Code;
        public readonly string Id;

        ActorPath(string code, string id)
        {
            Code = code;
            Id = id;
        }

        public static ActorPath From(Type type, string id)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (id == null)
                throw new ArgumentNullException("id");

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An actor id cannot be empty or contain whitespace only", "id");

            return new ActorPath(TypeCode.Find(type), id);
        }

        public static ActorPath From(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid actor path: " + path);

            return new ActorPath(parts[0], parts[1]);
        }

        internal static void Register(Type type, string code)
        {
            TypeCode.Cache(type, code);
        }

        public Type RuntimeType()
        {
            return TypeCode.Find(Code);
        }

        public bool Equals(ActorPath other)
        {
            return string.Equals(Code, other.Code) && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is ActorPath && Equals((ActorPath) obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Code != null ? Code.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ActorPath left, ActorPath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorPath left, ActorPath right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}", Code, Separator[0], Id);
        }

        static class TypeCode
        {
            static readonly Dictionary<string, Type> codeMap =
                        new Dictionary<string, Type>();

            static readonly Dictionary<Type, string> typeMap =
                        new Dictionary<Type, string>();

            internal static void Cache(Type type, string code)
            {
                if (codeMap.ContainsKey(code))
                {
                    var existing = codeMap[code];

                    if (existing != type)
                        throw new ArgumentException(
                            string.Format("The type {0} has been already registered under the code {1}. Use TypeCode attribute to provide unique code for {2}",
                                          existing.FullName, code, type.FullName));

                    throw new ArgumentException(string.Format("The type {0} has been already registered", type));
                }

                codeMap.Add(code, type);
                typeMap.Add(type, code);
            }

            internal static Type Find(string code)
            {
                Type type;
                
                if (!codeMap.TryGetValue(code, out type))
                    throw new InvalidOperationException(
                        string.Format("Unable to map type code '{0}' to the corresponding runtime type. Make sure that you've registered the assembly containing this type", code));

                return type;
            }            
            
            internal static string Find(Type type)
            {
                string code;

                if (!typeMap.TryGetValue(type, out code))
                    throw new InvalidOperationException(
                        string.Format("Unable to map type '{0}' to the corresponding type code. Make sure that you've registered the assembly containing this type", type));

                return code;
            }
        }
    }
}
