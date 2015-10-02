using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka
{
    using Core;
    using Core.Endpoints;
    using Utility;

    [Serializable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef : ObserverRef, IEquatable<ActorRef>, IEquatable<ActorPath>, ISerializable
    {
        public static ActorRef Deserialize(ActorPath path)
        {
            return new ActorRef(path, ActorEndpoint.Proxy(path));
        }

        readonly IActorEndpoint endpoint;
        readonly ActorInterface @interface;

        protected internal ActorRef(ActorPath path)
        {
            Path = path;
        }

        ActorRef(ActorPath path, IActorEndpoint endpoint) : this(path)
        {
            this.endpoint = endpoint;
            @interface = ActorInterface.Of(path);
        }

        public ActorPath Path { get; }

        public override string Serialize()
        {
            return Path.Serialize();
        }

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, nameof(message));

            return Receive(message)(new RequestEnvelope(Serialize(), message))
                    .UnwrapExceptions();
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, nameof(message));

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
            if (@interface.IsReentrant(message))
                return endpoint.ReceiveReentrant;

            return endpoint.Receive;
        }

        public bool Equals(ActorRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || Path.Equals(other.Path));
        }

        public bool Equals(ActorPath other)
        {
            return Path.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj.GetType() == GetType() && Equals((ActorRef) obj));
        }

        public override int GetHashCode() => Path.GetHashCode();

        public static bool operator ==(ActorRef left, ActorRef right) => Equals(left, right);
        public static bool operator !=(ActorRef left, ActorRef right) => !Equals(left, right);

        public override string ToString() => Path.ToString();

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", Path.Serialize(), typeof(string));
        }

        public ActorRef(SerializationInfo info, StreamingContext context)
        {
            var value = (string) info.GetValue("path", typeof(string));
            Path = ActorPath.Deserialize(value);
            endpoint = ActorEndpoint.Proxy(Path);
            @interface = ActorInterface.Of(Path);
        }

        #endregion
    }
}