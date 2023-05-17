using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;

namespace Orleankka
{
    using Meta;
    using Utility;

    [Serializable, Immutable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef : ObserverRef, IEquatable<ActorRef>, IEquatable<ActorPath>
    {
        [NonSerialized] readonly IActorGrain endpoint;
        [NonSerialized] readonly IActorRefMiddleware middleware;

        protected ActorRef(ActorPath path)
        {
            Path = path;
        }

        internal ActorRef(ActorPath path, IActorGrain endpoint, IActorRefMiddleware middleware)
            : this(path)
        {
            this.endpoint = endpoint;
            this.middleware = middleware;
        }

        public ActorPath Path { get; }

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, nameof(message));

            return middleware.Receive(Path, message, async x =>
            {
                await endpoint.ReceiveTell(x);
                return null;
            });
        }

        public virtual async Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, nameof(message));

            return (TResult) await middleware.Receive(Path, message, endpoint.ReceiveAsk);
        }

        public override void Notify(object message)
        {
            Requires.NotNull(message, nameof(message));

            middleware.Receive(Path, message, async x =>
            {
                await endpoint.ReceiveNotify(x);
                return null;
            });
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

        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public static implicit operator GrainReference(ActorRef arg) => (GrainReference) arg.endpoint;
        public static implicit operator ActorPath(ActorRef arg) => arg.Path;

        public static Task operator <(ActorRef @ref, Message message) => @ref.Tell(message);
        public static Task operator >(ActorRef @ref, Message message) => throw new NotImplementedException();
    }

    public interface IStronglyTypedActorRef {}

    [Serializable, Immutable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef<TActor> : ObserverRef<TActor>, IEquatable<ActorRef<TActor>>, IEquatable<ActorPath>, IStronglyTypedActorRef where TActor : IActorGrain, IGrainWithStringKey
    {
        readonly ActorRef @ref;

        protected internal ActorRef(ActorRef @ref)
        {
            this.@ref = @ref;
        }

        public virtual Task Tell(ActorMessage<TActor> message) => @ref.Tell(message);
        public virtual Task<TResult> Ask<TResult>(ActorMessage<TActor, TResult> message) => @ref.Ask<TResult>(message);
        public virtual Task<TResult> Ask<TResult>(ActorMessage<TActor> message) => @ref.Ask<TResult>(message);
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
        public static implicit operator ActorPath(ActorRef<TActor> arg) => arg.Path;

        public static Task operator <(ActorRef<TActor> @ref, ActorMessage<TActor> message) => @ref.Tell(message);
        public static Task operator >(ActorRef<TActor> @ref, ActorMessage<TActor> message) => throw new NotImplementedException();
    }
}