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

            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An actor id cannot be empty or contain whitespace only", nameof(id));

            return new ActorPath(ActorType.Of(type), id);
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

            return new ActorPath(ActorType.Registered(code), id);
        }

        public static ActorPath Deserialize(string path)
        {
            var parts = path.Split(Separator, 2, StringSplitOptions.None);

            var code = parts[0];
            var id = parts[1];

            return new ActorPath(ActorType.Registered(code), id);
        }

        readonly ActorType type;
        readonly string id;

        ActorPath(ActorType type, string id)
        {
            this.type = type;
            this.id = id;
        }

        public string Id => id;
        public string Code => type.Code;
        internal ActorType Type => type;

        public string Serialize()
        {
            return $"{Code}{Separator[0]}{id}";
        }

        public bool Equals(ActorPath other)
        {
            return type == other.type && string.Equals(id, other.id);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is ActorPath && Equals((ActorPath)obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((type?.GetHashCode() ?? 0) * 397) ^
                        (id?.GetHashCode() ?? 0);
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