using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.CodeGeneration;
using Orleans.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka
{
    using Utility;

    [Serializable, Immutable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef : ObserverRef, IEquatable<ActorRef>, IEquatable<ActorPath>
    {
        [NonSerialized] readonly IActorGrain endpoint;
        [NonSerialized] readonly IActorRefInvoker invoker;

        protected ActorRef(ActorPath path)
        {
            Path = path;
        }

        internal ActorRef(ActorPath path, IActorGrain endpoint, IActorRefInvoker invoker)
            : this(path)
        {
            this.endpoint = endpoint;
            this.invoker = invoker;
        }

        public ActorPath Path { get; }

        public virtual Task Tell(object message)
        {
            Requires.NotNull(message, nameof(message));

            return invoker.Send<object>(Path, message, async x =>
            {
                await endpoint.ReceiveTell(x);
                return null;
            });
        }

        public virtual Task<TResult> Ask<TResult>(object message)
        {
            Requires.NotNull(message, nameof(message));

            return invoker.Send<TResult>(Path, message, endpoint.ReceiveAsk);
        }

        public override void Notify(object message)
        {
            Requires.NotNull(message, nameof(message));

            invoker.Send<object>(Path, message, async x =>
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

        #region Orleans Native Serialization

        [CopierMethod]
        static object Copy(object input, ICopyContext context) => input;

        [SerializerMethod]
        static void Serialize(object input, ISerializationContext context, Type expected)
        {
            var writer = context.StreamWriter;
            var @ref = (ActorRef)input;
            writer.Write(@ref.Path);
        }

        [DeserializerMethod]
        static object Deserialize(Type t, IDeserializationContext context)
        {
            var reader = context.StreamReader;
            var path = ActorPath.Parse(reader.ReadString());
            var system = context.ServiceProvider.GetRequiredService<IActorSystem>();
            return system.ActorOf(path);
        }

        #endregion
    }

    [Serializable, Immutable]
    [DebuggerDisplay("a->{ToString()}")]
    public class ActorRef<TActor> : ObserverRef<TActor>, IEquatable<ActorRef<TActor>>, IEquatable<ActorPath> where TActor : IActorGrain
    {
        readonly ActorRef @ref;

        protected internal ActorRef(ActorRef @ref)
        {
            this.@ref = @ref;
        }

        public virtual Task Tell(ActorMessage<TActor> message) => @ref.Tell(message);
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
        public static implicit operator ActorPath(ActorRef<TActor> arg) => arg.Path;
    }
}