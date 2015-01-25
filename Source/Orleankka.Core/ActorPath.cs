using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    public sealed class ActorPath : IEquatable<ActorPath>
    {
        public readonly Type Type;
        public readonly string Id;

        public static ActorPath Of(Type type, string id)
        {
            Requires.NotNull(type, "type");
            Requires.NotNullOrWhitespace(id, "id");
            
            return new ActorPath(type, id);
        }

        internal ActorPath(Type type, string id)
        {           
            Type = type;
            Id = id;
        }

        public bool Equals(ActorPath other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || Type == other.Type && string.Equals(Id, other.Id));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj is ActorPath && Equals((ActorPath) obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() * 397) ^ Id.GetHashCode();
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
    }
}
