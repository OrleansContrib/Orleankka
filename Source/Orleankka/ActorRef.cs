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
            return new ActorRef(path, ActorEndpoint.Invoker(path));
        }

        readonly ActorPath path;
        readonly ActorEndpointInvoker invoker;
        readonly object endpoint;

        protected internal ActorRef(ActorPath path)
        {
            this.path = path;
        }

        ActorRef(ActorPath path, ActorEndpointInvoker invoker) : this(path)
        {
            this.invoker = invoker;
            endpoint = invoker.GetProxy(path.Serialize());
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

            return invoker
                    .ReceiveTell(endpoint, new RequestEnvelope(Serialize(), message))
                    .UnwrapExceptions();
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, "message");

            var response = await invoker
                    .ReceiveAsk(endpoint, new RequestEnvelope(Serialize(), message))
                    .UnwrapExceptions();

            return (TResult) response.Result;
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
            invoker = ActorEndpoint.Invoker(path);
            endpoint = invoker.GetProxy(value);
        }

        #endregion
    }
}