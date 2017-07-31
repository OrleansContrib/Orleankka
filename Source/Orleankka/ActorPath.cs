using System;
using System.Diagnostics;

using Orleans.Concurrency;

namespace Orleankka
{
    using Utility;
     
    [Serializable, Immutable]
    [DebuggerDisplay("{ToString()}")]
    public struct ActorPath : IEquatable<ActorPath>
    {
        public static readonly ActorPath Empty = new ActorPath();
        public static readonly string[] Separator = {":"};

        public static ActorPath From(string type, string id)
        {
            Requires.NotNull(type, nameof(type));
            Requires.NotNull(id, nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An actor id cannot be empty or contain whitespace only", nameof(id));

            return new ActorPath(type, id);
        }

        public static ActorPath Parse(string path)
        {
            Requires.NotNull(path, nameof(path));

            var parts = path.Split(Separator, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid actor path: " + path);

            var type = parts[0];
            var id = parts[1];

            return new ActorPath(type, id);
        }

        public readonly string Type;
        public readonly string Id;

        internal ActorPath(string type, string id)
        {
            Type = type;
            Id = id;
        }

        public bool Equals(ActorPath other) => Type == other.Type && string.Equals(Id, other.Id);
        public override bool Equals(object obj) => !ReferenceEquals(null, obj) && (obj is ActorPath && Equals((ActorPath)obj));

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type?.GetHashCode() ?? 0) * 397) ^
                        (Id?.GetHashCode() ?? 0);
            }
        }

        public static implicit operator string(ActorPath arg) => arg.ToString();

        public static bool operator ==(ActorPath left, ActorPath right) => Equals(left, right);
        public static bool operator !=(ActorPath left, ActorPath right) => !Equals(left, right);

        public override string ToString() => $"{Type}:{Id}";
    }
}