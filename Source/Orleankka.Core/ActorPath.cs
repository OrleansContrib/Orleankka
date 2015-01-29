using System;
using System.Diagnostics;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    [DebuggerDisplay("{TypeCode}::{Id}")]
    public struct ActorPath : IEquatable<ActorPath>
    {
        static readonly string[] separator = { "::" };
        
        public readonly string TypeCode;
        public readonly string Id;

        public static readonly ActorPath Empty = new ActorPath();

        public ActorPath(string typeCode, string id)
        {
            Requires.NotNullOrWhitespace(typeCode, "typeCode");
            Requires.NotNullOrWhitespace(id, "id");

            if (typeCode.Contains(separator[0]))
                throw new ArgumentException(
                    string.Format("Type code cannot contain '{0}' chars", separator[0]));
            
            TypeCode = typeCode;
            Id = id;
        }

        public static ActorPath From(Type type, string id)
        {
            Requires.NotNull(type, "type");
            return new ActorPath(TypeCodeOf(type), id);
        }

        public static ActorPath From(string path)
        {
            Requires.NotNullOrWhitespace(path, "path");

            var parts = path.Split(separator, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid actor path: " + path);
            
            return new ActorPath(parts[0], parts[1]);
        }

        internal static string TypeCodeOf(Type type)
        {
            return type.Name; // TODO: check TypeCodeOverride attribute
        }

        public bool Equals(ActorPath other)
        {
            return string.Equals(TypeCode, other.TypeCode) && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is ActorPath && Equals((ActorPath) obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((TypeCode != null ? TypeCode.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
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

        public static implicit operator string(ActorPath arg)
        {
            return arg.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0}{1}{2}", TypeCode, separator[0], Id);
        }
    }
}
