using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;
    using Utility;
    
    [Serializable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef : ObserverRef, IEquatable<ActorRef>, IEquatable<ActorPath>, ISerializable
    {
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

        public override string Serialize()
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

        public override void Notify(object message)
        {
            Tell(message).Ignore();
        }

        Func<RequestEnvelope, Task<ResponseEnvelope>> Receive(object message)
        {
            return ActorPrototype.Of(Path.Type).IsReentrant(message) 
                       ? (Func<RequestEnvelope, Task<ResponseEnvelope>>) endpoint.ReceiveReentrant 
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