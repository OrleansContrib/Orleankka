using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleankka
{
    using Core;
    using Utility;
    
    [Serializable, Immutable]
    [DebuggerDisplay("{ToString()}")]
    public class ClientRef : ObserverRef, IEquatable<ClientRef>, IEquatable<string>
    {
        public static ClientRef Deserialize(string path, IGrainFactory factory)
        {
            // TODO: Fixit
            return new ClientRef(path);
        }

        readonly IClientEndpoint endpoint;

        protected internal ClientRef(string path)
        {
            Path = path;
        }

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        internal ClientRef(IClientEndpoint endpoint) 
            : this(((GrainReference)endpoint).ToKeyString())
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

        public static bool operator ==(ClientRef left, ClientRef right) => Equals(left, right);
        public static bool operator !=(ClientRef left, ClientRef right) => !Equals(left, right);

        public override string ToString() => Path;
    }
}