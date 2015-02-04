using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Orleankka
{
    using Core;

    [Serializable]
    [DebuggerDisplay("{DebuggerDisplay()}")]
    public class ObserverRef : IEquatable<ObserverRef>, IEquatable<string>, ISerializable
    {
        public static ObserverRef Resolve(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An observer path cannot be empty or contain whitespace only", "path");

            return Deserialize(path);
        }
        
        public static ObserverRef Deserialize(string path)
        {
            return new ObserverRef(path, ObserverEndpoint.Proxy(path));
        }

        readonly string path;
        readonly IObserverEndpoint endpoint;

        protected internal ObserverRef(string path)
        {
            this.path = path;
        }

        ObserverRef(string path, IObserverEndpoint endpoint) : this(path)
        {
            this.endpoint = endpoint;
        }

        public string Path
        {
            get { return path; }
        }

        public string Serialize()
        {
            return Path;
        }

        public virtual void Notify(Notification notification)
        {
            var envelope = new NotificationEnvelope(
                notification.Sender.Serialize(), 
                notification.Message
            );

            endpoint.ReceiveNotify(envelope);
        }

        public bool Equals(ObserverRef other)
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
                    || obj.GetType() == GetType() && Equals((ObserverRef)obj));
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }

        public static bool operator ==(ObserverRef left, ObserverRef right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObserverRef left, ObserverRef right)
        {
            return !Equals(left, right);
        }

        internal string DebuggerDisplay()
        {
            return Serialize();
        }

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", path, typeof(string));
        }

        public ObserverRef(SerializationInfo info, StreamingContext context)
        {
            path = (string) info.GetValue("path", typeof(string));
            endpoint = ObserverEndpoint.Proxy(path);
        }

        #endregion
    }
}