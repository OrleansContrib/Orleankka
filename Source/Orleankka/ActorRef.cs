using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core;

    [Serializable]
    [DebuggerDisplay("a->{Path}")]
    public class ActorRef : IEquatable<ActorRef>, IEquatable<ActorPath>, ISerializable
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
        readonly ActorEndpointInvoker invoker;
        readonly object endpoint;

        protected internal ActorRef(ActorPath path)
        {
            this.path = path;
        }

        internal ActorRef(ActorPath path, ActorEndpointInvoker invoker) 
            : this(path)
        {
            this.invoker = invoker;
            endpoint = invoker.GetProxy(path.ToString());
        }

        public ActorPath Path
        {
            get { return path; }
        }

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, "message");

            return invoker
                    .ReceiveTell(endpoint, new RequestEnvelope(path, message))
                    .UnwrapExceptions();
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, "message");

            var response = await invoker
                    .ReceiveAsk(endpoint, new RequestEnvelope(path, message))
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

        #region Serialization

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("path", path.ToString(), typeof(string));
        }

        public ActorRef(SerializationInfo info, StreamingContext context)
        {
            path = ActorPath.From((string) info.GetValue("path", typeof(string)));
            invoker = ActorEndpoint.Invoker(path);
            endpoint = invoker.GetProxy(path.ToString());
        }

        #endregion
    }

    public static class ActorRefExtensions
    {
        public static Task<object> Ask(this ActorRef @ref, object message)
        {
            return @ref.Ask<object>(message);
        }
    }
}