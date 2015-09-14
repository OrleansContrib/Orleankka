using System;
using System.Diagnostics;

namespace Orleankka
{
    using Core;

    [DebuggerDisplay("{ToString()}")]
    public struct ActorPath : IEquatable<ActorPath>
    {
        public static readonly ActorPath Empty = new ActorPath();
        public static readonly string[] Separator = { ":" };

        public static ActorPath From(Type type, string id)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return From(ActorType.Of(type).Code, id);
        }

        public static ActorPath From(string code, string id)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An actor id cannot be empty or contain whitespace only", nameof(id));

            return new ActorPath(code, id);
        }

        public static ActorPath Parse(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid actor path: " + path);

            var code = parts[0];
            var id = parts[1];

            return new ActorPath(code, id);
        }

        public static ActorPath Deserialize(string path)
        {
            var parts = path.Split(Separator, 2, StringSplitOptions.None);

            var code = parts[0];
            var id = parts[1];

            return new ActorPath(code, id);
        }

        public readonly string Code;
        public readonly string Id;

        ActorPath(string code, string id)
        {
            Code = code;
            Id = id;
        }

        public string Serialize()
        {
            return $"{Code}{Separator[0]}{Id}";
        }

        public bool Equals(ActorPath other)
        {
            return Code == other.Code && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is ActorPath && Equals((ActorPath)obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Code?.GetHashCode() ?? 0) * 397) ^
                        (Id?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(ActorPath left, ActorPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActorPath left, ActorPath right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}