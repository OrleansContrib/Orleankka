using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Orleankka
{
    using Core;

    [Serializable]
    [DebuggerDisplay("o->{Path}")]
    public class ObserverRef : IEquatable<ObserverRef>, IEquatable<ObserverPath>, ISerializable
    {
        public static ObserverRef Resolve(string path)
        {
            return Resolve(ObserverPath.Parse(path));
        }

        public static ObserverRef Resolve(ObserverPath path)
        {
            if (path == ObserverPath.Empty)
                throw new ArgumentException("ObserverPath is empty", "path");

            return Deserialize(path);
        }
        
        public static ObserverRef Deserialize(string path)
        {
            return Deserialize(ObserverPath.Deserialize(path));
        }
        
        public static ObserverRef Deserialize(ObserverPath path)
        {
            return new ObserverRef(path, ObserverEndpoint.Proxy(path));
        }

        readonly ObserverPath path;
        readonly IObserverEndpoint endpoint;

        protected internal ObserverRef(ObserverPath path)
        {
            this.path = path;
        }

        ObserverRef(ObserverPath path, IObserverEndpoint endpoint) : this(path)
        {
            this.endpoint = endpoint;
        }

        public ObserverPath Path
        {
            get { return path; }
        }

        public string Serialize()
        {
            return Path.Serialize();
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

        public bool Equals(ObserverPath other)
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

        public override string ToString()
        {
            return Serialize();
        }

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", path.Serialize(), typeof(string));
        }

        public ObserverRef(SerializationInfo info, StreamingContext context)
        {
            var value = (string) info.GetValue("path", typeof(string));
            path = ObserverPath.Deserialize(value);
            endpoint = ObserverEndpoint.Proxy(path);
        }

        #endregion
    }
}