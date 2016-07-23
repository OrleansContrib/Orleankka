using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka.CSharp
{
    [Serializable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef<TActor> : ObserverRef<TActor>, IEquatable<ActorRef<TActor>>, IEquatable<ActorPath>, ISerializable where TActor : IActor
    {
        static ActorRef<TActor> Deserialize(string path) => Deserialize(ActorPath.Deserialize(path));
        static ActorRef<TActor> Deserialize(ActorPath path) => new ActorRef<TActor>(ActorRef.Deserialize(path));

        readonly ActorRef @ref;

        protected internal ActorRef(ActorRef @ref)
        {
            this.@ref = @ref;
        }

        public virtual Task Tell(ActorMessage<TActor> message) => @ref.Tell(message);
        public virtual Task<TResult> Ask<TResult>(ActorMessage<TActor> message) => @ref.Ask<TResult>(message);
        public virtual Task<TResult> Ask<TResult>(ActorMessage<TActor, TResult> message) => @ref.Ask<TResult>(message);
        public override void Notify(ActorMessage<TActor> message) => @ref.Notify(message);

        public ActorPath Path => @ref.Path;

        public bool Equals(ActorRef<TActor> other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other)
                    || Path.Equals(other.Path));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj)
                    || obj.GetType() == GetType() && Equals((ActorRef<TActor>)obj));
        }

        public bool Equals(ActorPath other) => Path.Equals(other);
        public override string ToString() => Path.ToString();
        public override int GetHashCode() => Path.GetHashCode();

        public static bool operator ==(ActorRef<TActor> left, ActorRef<TActor> right) => Equals(left, right);
        public static bool operator !=(ActorRef<TActor> left, ActorRef<TActor> right) => !Equals(left, right);

        public static implicit operator ActorRef(ActorRef<TActor> arg) => arg.@ref;
        public static implicit operator ActorRef<TActor>(ActorRef arg) => new ActorRef<TActor>(arg);
        public static implicit operator GrainReference(ActorRef<TActor> arg) => arg.@ref;

        #region Default Binary Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", @ref.Path.Serialize(), typeof(string));
        }

        public ActorRef(SerializationInfo info, StreamingContext context)
        {
            @ref = Deserialize(info.GetString("path"));
        }

        #endregion
    }
}