using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    public sealed class ActorPath : IEquatable<ActorPath>
    {
        static readonly ConcurrentDictionary<Type, Type> actorInterfaceMap = 
                    new ConcurrentDictionary<Type, Type>();

        internal static ActorPath Map(Type type, string id)
        {
            return new ActorPath(ActorInterfaceOf(type), id);
        }

        static Type ActorInterfaceOf(Type type)
        {
            return actorInterfaceMap.GetOrAdd(type, t =>
            {
                var found = t.GetInterfaces()
                    .Except(t.GetInterfaces().SelectMany(x => x.GetInterfaces()))
                    .Where(x => typeof(IActor).IsAssignableFrom(x))
                    .Where(x => x != typeof(IActor))
                    .ToArray();

                Debug.Assert(found.Length <= 1, "WAT! How did this happen?");

                if (!found.Any()) 
                   throw new InvalidOperationException(
                       string.Format("The type '{0}' does not implement any of IActor inherited interfaces", t));

                return found[0];
            });
        }

        public readonly Type Type;
        public readonly string Id;

        public ActorPath(Type type, string id)
        {
            Requires.NotNullOrWhitespace(id, "id");

            if (!type.IsInterface || type == typeof(IActor) || !typeof(IActor).IsAssignableFrom(type))
                throw new ArgumentException("Type should be an interface which implements IActor", "type");

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
