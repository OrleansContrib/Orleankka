using System;
using System.Diagnostics;
using System.Linq;

namespace Orleankka
{
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

            return new ActorPath(ActorTypeCode.Find(type), id);
        }

        public static ActorPath Parse(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid actor path: " + path);

            return new ActorPath(parts[0], parts[1]);
        }

        public static ActorPath Deserialize(string path)
        {
            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            return new ActorPath(parts[0], parts[1]);
        }

        public string Serialize()
        {
            return string.Format("{0}{1}{2}", Code, Separator[0], Id);
        }

        internal Type RuntimeType()
        {
            return ActorTypeCode.Find(Code);
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
            return Serialize();
        }
    }
}
