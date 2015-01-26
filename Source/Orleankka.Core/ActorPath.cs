using System;
using System.Collections.Concurrent;
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

        public static ActorPath Of(string path)
        {
            return Serializer.Deserialize(path);
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

        public override string ToString()
        {
            return Serializer.Serialize(this);
        }

        static class Serializer
        {
            static readonly string[] separator = {"::"};

            static readonly ConcurrentDictionary<string, Type> cache =
                        new ConcurrentDictionary<string, Type>();

            public static string Serialize(ActorPath path)
            {
                return string.Format("{0}{1}{2}", path.Type.FullName, separator[0], path.Id);
            }
            
            public static ActorPath Deserialize(string path)
            {
                var parts = path.Split(separator, 2, StringSplitOptions.None);
                return new ActorPath(Find(parts[0]), parts[1]);
            }

            static Type Find(string fullName)
            {
                return cache.GetOrAdd(fullName, n =>
                {
                    var candidates = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .Where(x => x.FullName == n)
                        .ToArray();

                    if (candidates.Length > 1)
                        throw new InvalidOperationException("Multiple types match the given type full name: " + n);

                    if (candidates.Length == 0)
                        throw new InvalidOperationException("Can't find type its by full name: " + n);

                    return candidates[0];
                });
            }
        }
    }
}
