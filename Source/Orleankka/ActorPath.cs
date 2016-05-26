using System;
using System.Diagnostics;

using Orleankka.Core;

namespace Orleankka
{
    using Utility;
     
    [Serializable]
    [DebuggerDisplay("{ToString()}")]
    public struct ActorPath : IEquatable<ActorPath>
    {
        public static readonly ActorPath Empty = new ActorPath();
        public static readonly string[] Separator = {":"};

        public static ActorPath From(string code, string id)
        {
            Requires.NotNull(code, nameof(code));
            Requires.NotNull(id, nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("An actor id cannot be empty or contain whitespace only", nameof(id));

            return new ActorPath(code, id);
        }

        public static ActorPath Parse(string path)
        {
            Requires.NotNull(path, nameof(path));

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

        public static bool operator ==(ActorPath left, ActorPath right) => Equals(left, right);
        public static bool operator !=(ActorPath left, ActorPath right) => !Equals(left, right);

        public override string ToString() => Serialize();
    }

    /// <summary>
    /// This should live in the server-side assembly
    /// </summary>
    public static class ActorPathExtensions
    {
        public static ActorPath ToActorPath(this Type type, string id)
        {
            Requires.NotNull(type, nameof(type));
            var code = ActorTypeCode.Of(type);
            return ActorPath.From(code, id);
        }
    }
}