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
        internal static bool Satisfies(string path)
        {
            return path.StartsWith("GrainReference=");
        }

        public new static ClientRef Deserialize(string path)
        {
            return new ClientRef(path, ClientEndpoint.Proxy(path));
        }

        readonly string path;
        readonly IClientEndpoint endpoint;

        protected internal ClientRef(string path)
        {
            this.path = path;
        }

        ClientRef(string path, IClientEndpoint endpoint)
            : this(path)
        {
            this.endpoint = endpoint;
        }

        public string Path
        {
            get { return path; }
        }

        public override string Serialize()
        {
            return Path;
        }

        public override void Notify(object message)
        {
            Requires.NotNull(message, "message");

            endpoint.Receive(new NotificationEnvelope(message));
        }

        public bool Equals(ClientRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || Equals(path, other.path));
        }

        public bool Equals(string other)
        {
            return path.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType() && Equals((ClientRef)obj));
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }

        public static bool operator ==(ClientRef left, ClientRef right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClientRef left, ClientRef right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Path;
        }

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", path, typeof(string));
        }

        public ClientRef(SerializationInfo info, StreamingContext context)
        {
            path = (string)info.GetValue("path", typeof(string));
            endpoint = ClientEndpoint.Proxy(path);
        }

        #endregion
    }
}