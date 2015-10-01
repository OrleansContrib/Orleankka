using System;
using System.Collections.Generic;
using System.Diagnostics;

using Orleans;
using Orleans.Providers;
using Orleans.Streams;

namespace Orleankka
{
    using Utility;

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
            Requires.NotNull(type, nameof(type));
            Requires.NotNull(id, nameof(id));
            Requires.NotNullOrWhitespace(id, nameof(id));

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

            throw new InvalidOperationException($"Can't find stream provider of specified stream type: {Type}");
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
                return ((Type?.GetHashCode() ?? 0) * 397) ^ (Id?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(StreamPath left, StreamPath right) => left.Equals(right);
        public static bool operator !=(StreamPath left, StreamPath right) => !left.Equals(right);
    }
}
