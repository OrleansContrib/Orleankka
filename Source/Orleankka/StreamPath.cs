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
        
        public readonly string Provider;
        public readonly string Id;

        StreamPath(string provider, string id)
        {
            Provider = provider;
            Id = id;
        }

        public static StreamPath From(string provider, string id)
        {
            Requires.NotNull(provider, nameof(provider));
            Requires.NotNull(id, nameof(id));
            Requires.NotNullOrWhitespace(id, nameof(id));

            return new StreamPath(provider, id);
        }

        internal IAsyncStream<object> Proxy()
        {
            var provider = GrainClient.GetStreamProvider(Provider);
            return provider.GetStream<object>(Guid.Empty, Id);
        }

        public bool Equals(StreamPath other)
        {
            return Provider == other.Provider && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is StreamPath && Equals((StreamPath)obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Provider?.GetHashCode() ?? 0) * 397) ^ (Id?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(StreamPath left, StreamPath right) => left.Equals(right);
        public static bool operator !=(StreamPath left, StreamPath right) => !left.Equals(right);
    }
}
