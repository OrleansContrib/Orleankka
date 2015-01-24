using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    public sealed class ActorObserverPath : IEquatable<ActorObserverPath>
    {
        public readonly string Id;

        public ActorObserverPath(string id)
        {
            Requires.NotNullOrWhitespace(id, "id");
            Id = id;
        }

        public static implicit operator string(ActorObserverPath arg)
        {
            return arg.Id;
        }

        public bool Equals(ActorObserverPath other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(Id, other.Id));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj is ActorObserverPath && Equals((ActorObserverPath) obj));
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(ActorObserverPath left, ActorObserverPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActorObserverPath left, ActorObserverPath right)
        {
            return !Equals(left, right);
        }
    }
}
