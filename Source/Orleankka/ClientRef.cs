using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Orleankka
{
    using Core;
    using Utility;
    
    [Serializable]
    [DebuggerDisplay("{ToString()}")]
    public class ClientRef : ObserverRef, IEquatable<ClientRef>, IEquatable<string>, ISerializable
    {
        public static ClientRef Deserialize(string path) => new ClientRef(path, ClientEndpoint.Proxy(path));

        readonly IClientEndpoint endpoint;

        protected internal ClientRef(string path)
        {
            Path = path;
        }

        ClientRef(string path, IClientEndpoint endpoint)
            : this(path)
        {
            this.endpoint = endpoint;
        }

        public string Path { get; }
        public override string Serialize() => Path;

        public override void Notify(object message)
        {
            Requires.NotNull(message, nameof(message));
            endpoint.Receive(new NotificationEnvelope(message));
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

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", Path, typeof(string));
        }

        public ClientRef(SerializationInfo info, StreamingContext context)
        {
            Path = (string)info.GetValue("path", typeof(string));
            endpoint = ClientEndpoint.Proxy(Path);
        }

        #endregion
    }
}