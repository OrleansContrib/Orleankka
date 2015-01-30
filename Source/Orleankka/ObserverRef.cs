using System;
using System.Diagnostics;
using System.Linq;

using Orleankka.Core;

namespace Orleankka
{
    [Serializable]
    [DebuggerDisplay("o->{Path}")]
    public class ObserverRef : IEquatable<ObserverRef>
    {
        public static ObserverRef Resolve(string path)
        {
            return ActorSystem.Instance.ObserverOf(ObserverPath.From(path));
        }

        readonly ObserverPath path;
        readonly IObserverEndpoint endpoint;

        protected ObserverRef(ObserverPath path)
        {
            this.path = path;
        }

        internal ObserverRef(ObserverPath path, IObserverEndpoint endpoint) 
            : this(path)
        {
            this.endpoint = endpoint;
        }

        public ObserverPath Path
        {
            get { return path; }
        }

        public virtual void Notify(Notification notification)
        {
            var envelope = new NotificationEnvelope(
                notification.Sender.Path, 
                notification.Message
            );

            endpoint.ReceiveNotify(envelope);
        }

        public bool Equals(ObserverRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other)
                    || path.Equals(other.path));
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
            return Path.ToString();
        }
    }
}