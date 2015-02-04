using System;
using System.Diagnostics;
using System.Linq;

namespace Orleankka
{
    [DebuggerDisplay("{ToString()}")]
    public struct ActorPath : IEquatable<ActorPath>
    {
        public static readonly ActorPath Empty = new ActorPath();
        public static readonly string[] Separator = {"/"};
        
        public readonly Type Type;
        public readonly string Id;

        ActorPath(Type type, string id)
        {
            Type = type;
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

            return new ActorPath(type, id);
        }

        public static ActorPath Parse(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid actor path: " + path);

            var code = parts[0];
            var id = parts[1];

            return new ActorPath(RegisteredType(code), id);
        }

        public static ActorPath Deserialize(string path)
        {
            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            
            var code = parts[0];
            var id = parts[1];

            return new ActorPath(RegisteredType(code), id);
        }

        public string Serialize()
        {
            return Serialize(RegisteredCode(Type));
        }
        
        string Serialize(string code)
        {
            return string.Format("{0}{1}{2}", code, Separator[0], Id);
        }

        static Type RegisteredType(string code)
        {
            return ActorTypeCode.RegisteredType(code);
        }

        static string RegisteredCode(Type type)
        {
            return ActorTypeCode.RegisteredCode(type);
        }

        public bool Equals(ActorPath other)
        {
            return Type == other.Type && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is ActorPath && Equals((ActorPath) obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
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
            return Serialize(ActorTypeCode.CodeOf(Type));
        }
    }
}
