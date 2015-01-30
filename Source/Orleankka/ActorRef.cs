using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Orleankka.Core;

namespace Orleankka
{
    [Serializable]
    [DebuggerDisplay("a->{Path}")]
    public class ActorRef : IEquatable<ActorRef>, IEquatable<ActorPath>
    {
        public static ActorRef Resolve(string path)
        {
            return Resolve(ActorPath.From(path));
        }

        public static ActorRef Resolve(ActorPath path)
        {
            return ActorSystem.Instance.ActorOf(path);
        }

        readonly ActorPath path;
        readonly IActorEndpoint endpoint;

        protected ActorRef(ActorPath path)
        {
            this.path = path;
        }

        internal ActorRef(ActorPath path, IActorEndpoint endpoint) 
            : this(path)
        {
            this.endpoint = endpoint;
        }

        public ActorPath Path
        {
            get { return path; }
        }

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, "message");

            return endpoint
                    .ReceiveTell(new RequestEnvelope(path, message))
                    .UnwrapExceptions();
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, "message");

            var response = await endpoint
                    .ReceiveAsk(new RequestEnvelope(path, message))
                    .UnwrapExceptions();

            return (TResult) response.Message;
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
    }

    public static class ActorRefExtensions
    {
        public static Task<object> Ask(this ActorRef @ref, object message)
        {
            return @ref.Ask<object>(message);
        }
    }
}