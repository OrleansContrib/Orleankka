using System;
using System.Collections.Generic;
using System.Diagnostics;

using Orleans;
using Orleans.Providers;
using Orleans.Streams;

namespace Orleankka
{
    public struct StreamPath : IEquatable<StreamPath>
    {
        static readonly ICollection<IProviderConfiguration> providers =
                     new LinkedList<IProviderConfiguration>();

        internal static void Register(IEnumerable<IProviderConfiguration> providers)
        {
            Debug.Assert(StreamPath.providers.Count == 0);
            foreach (var each in providers)
                StreamPath.providers.Add(each);
        }

        internal static void Reset()
        {
            providers.Clear();
        }

        public static readonly StreamPath Empty = new StreamPath();
        
        public readonly Type Type;
        public readonly string Id;

        StreamPath(Type type, string id)
        {
            Type = type;
            Id = id;
        }

        public static StreamPath From(Type type, string id)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (id == null)
                throw new ArgumentNullException("id");

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("A stream id cannot be empty or contain whitespace only", "id");

            return new StreamPath(type, id);
        }

        internal IAsyncStream<object> Proxy()
        {
            foreach (var each in providers)
            {
                if (each.Type != Type.FullName)
                    continue;

                var provider = GrainClient.GetStreamProvider(each.Name);
                return provider.GetStream<object>(Guid.Empty, Id);
            }

            var message = string.Format("Can't find stream provider of specified stream type: {0}", Type);
            throw new InvalidOperationException(message);
        }

        public bool Equals(StreamPath other)
        {
            return Type == other.Type && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is StreamPath && Equals((StreamPath)obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }

        public static bool operator ==(StreamPath left, StreamPath right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StreamPath left, StreamPath right)
        {
            return !left.Equals(right);
        }
    }
}
