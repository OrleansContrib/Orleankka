using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;

    [Serializable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef : IEquatable<ActorRef>, IEquatable<ActorPath>, ISerializable
    {
        public static ActorRef Resolve(string path)
        {
            return Resolve(ActorPath.Parse(path));
        }

        public static ActorRef Resolve(ActorPath path)
        {
            if (path == ActorPath.Empty)
                throw new ArgumentException("ActorPath is empty", "path");

            return Deserialize(path);
        }

        public static ActorRef Deserialize(string path)
        {
            return Deserialize(ActorPath.Deserialize(path));
        }

        public static ActorRef Deserialize(ActorPath path)
        {
            return new ActorRef(path, ActorEndpoint.Proxy(path));
        }

        readonly ActorPath path;
        readonly IActorEndpoint endpoint;

        protected internal ActorRef(ActorPath path)
        {
            this.path = path;
        }

        ActorRef(ActorPath path, IActorEndpoint endpoint) : this(path)
        {
            this.endpoint = endpoint;
        }

        public ActorPath Path
        {
            get { return path; }
        }

        public string Serialize()
        {
            return Path.Serialize();
        }

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, "message");

            return Receive(message)(new RequestEnvelope(Serialize(), message))
                    .UnwrapExceptions();
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, "message");

            var response = await Receive(message)(new RequestEnvelope(Serialize(), message))
                    .UnwrapExceptions();

            return (TResult) response.Result;
        }

        Func<RequestEnvelope, Task<ResponseEnvelope>> Receive(object message)
        {
            return Message.Interleaved(message.GetType()) 
                       ? (Func<RequestEnvelope, Task<ResponseEnvelope>>) endpoint.ReceiveInterleave 
                       : endpoint.Receive;
        }

        public bool Equals(ActorRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || path.Equals(other.path));
        }

        public bool Equals(ActorPath other)
        {
            return path.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj.GetType() == GetType() && Equals((ActorRef) obj));
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }

        public static bool operator ==(ActorRef left, ActorRef right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActorRef left, ActorRef right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Path.ToString();
        }

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", path.Serialize(), typeof(string));
        }

        public ActorRef(SerializationInfo info, StreamingContext context)
        {
            var value = (string) info.GetValue("path", typeof(string));
            path = ActorPath.Deserialize(value);
            endpoint = ActorEndpoint.Proxy(path);
        }

        #endregion
    }
}