using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    using Core;
    using Core.Endpoints;
    using Utility;

    [Serializable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef : ObserverRef, IEquatable<ActorRef>, IEquatable<ActorPath>, ISerializable
    {
        public static ActorRef Deserialize(string path) => Deserialize(ActorPath.Deserialize(path));
        public static ActorRef Deserialize(ActorPath path) => new ActorRef(path, ActorType.Registered(path.Code));

        readonly IActorEndpoint endpoint;
        readonly ActorType type;

        protected internal ActorRef(ActorPath path)
        {
            Path = path;
        }

        ActorRef(ActorPath path, ActorType type)
            : this(path)
        {
            this.type = type;
            endpoint = this.type.Proxy(path);
        }

        public ActorPath Path { get; }
        public override string Serialize() => Path.Serialize();

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, nameof(message));

            return ReceiveVoid(message)(message);
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, nameof(message));

            var result = await Receive(message)(message);

            return (TResult) result;
        }

        public override void Notify(object message)
        {
            Tell(message).Ignore();
        }

        Func<object, Task<object>> Receive(object message)
        {
            if (type.IsReentrant(message))
                return endpoint.ReceiveReentrant;

            return endpoint.Receive;
        }

        Func<object, Task> ReceiveVoid(object message)
        {
            if (type.IsReentrant(message))
                return endpoint.ReceiveReentrantVoid;

            return endpoint.ReceiveVoid;
        }

        public bool Equals(ActorRef other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other)
                    || Path.Equals(other.Path));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType() && Equals((ActorRef) obj));
        }

        public bool Equals(ActorPath other) => Path.Equals(other);
        public override int GetHashCode()   => Path.GetHashCode();
        public override string ToString()   => Path.ToString();

        public static bool operator ==(ActorRef left, ActorRef right) => Equals(left, right);
        public static bool operator !=(ActorRef left, ActorRef right) => !Equals(left, right);

        public static implicit operator GrainReference(ActorRef arg) => (GrainReference) arg.endpoint;

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", Path.Serialize(), typeof(string));
        }

        public ActorRef(SerializationInfo info, StreamingContext context)
        {
            var value = (string) info.GetValue("path", typeof(string));
            Path = ActorPath.Deserialize(value);
            type = ActorType.Registered(Path.Code);
            endpoint = type.Proxy(Path);
        }

        #endregion
    }
}