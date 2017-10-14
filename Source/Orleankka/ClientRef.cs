using System;
using System.Diagnostics;

using Orleans.Concurrency;

namespace Orleankka
{
    using Core;
    using Utility;
    
    [Serializable, Immutable]
    [DebuggerDisplay("{ToString()}")]
    public class ClientRef : ObserverRef, IEquatable<ClientRef>, IEquatable<string>
    {
        readonly IClientEndpoint endpoint;

        internal ClientRef(string path)
        {
            Path = path;
        }

        internal ClientRef(IClientEndpoint endpoint) 
            : this(ClientEndpoint.Path(endpoint))
        {
            this.endpoint = endpoint;
        }

        public string Path { get; }

        public override void Notify(object message)
        {
            Requires.NotNull(message, nameof(message));
            endpoint.Receive(message);
        }

        public bool Equals(ClientRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || Equals(Path, other.Path));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType() && Equals((ClientRef)obj));
        }

        public bool Equals(string other)  => Path.Equals(other);
        public override int GetHashCode() => Path.GetHashCode();

        public static implicit operator string(ClientRef arg) => arg.ToString();

        public static bool operator ==(ClientRef left, ClientRef right) => Equals(left, right);
        public static bool operator !=(ClientRef left, ClientRef right) => !Equals(left, right);

        public override string ToString() => Path;
    }
}