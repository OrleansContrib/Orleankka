using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Core
{
    using Utility;
    using Endpoints;

    class ActorType : IEquatable<ActorType>
    {
        static readonly Dictionary<string, ActorType> codes =
                    new Dictionary<string, ActorType>();

        public static void Reset() => codes.Clear();

        public static void Register(IEnumerable<EndpointConfiguration> configs)
        {
            var actors = EndpointDeclaration.Generate(configs);

            foreach (var actor in actors)
                Register(actor);
       }

        static void Register(ActorType actor)
        {
            var registered = codes.Find(actor.Code);
            if (registered != null)
                throw new ArgumentException(
                    $"An actor with code '{actor.Code}' has been already registered");

            codes.Add(actor.Code, actor);
        }

        public readonly string Code;

        readonly TimeSpan keepAliveTimeout;
        readonly Func<object, bool> reentrant;
        readonly Func<string, object> factory;
        readonly Func<string, ActorContext, Func<IActorContext, object, Task<object>>> receiver;

        internal ActorType(string code, TimeSpan keepAliveTimeout, Func<object, bool> reentrant, Type @interface, Func<string, ActorContext, Func<IActorContext, object, Task<object>>> receiver)
        {
            Code = code;
            this.keepAliveTimeout = keepAliveTimeout;
            this.reentrant = reentrant;
            this.factory = Bind(@interface);
            this.receiver = receiver;
        }

        static Func<string, object> Bind(Type type)
        {
            var method = typeof(GrainFactory).GetMethod("GetGrain", new[] { typeof(string), typeof(string) });
            var invoker = method.MakeGenericMethod(type);
            var instance = Activator.CreateInstance(typeof(GrainFactory), nonPublic: true);
            return x => invoker.Invoke(instance, new object[] { x, null });
        }

        public static ActorType Registered(string code)
        {
            var result = codes.Find(code);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map code '{code}' to the corresponding actor type. " +
                     "Make sure that you've registered the assembly containing this type");

            return result;
        }

        internal IActorEndpoint Proxy(ActorPath path) => 
            (IActorEndpoint) factory(path.Serialize());

        public Func<ActorContext, object, Task<object>> Receiver(string id, ActorContext context) => 
            receiver(id, context);

        internal bool IsReentrant(object message) => 
            reentrant(message);

        internal void KeepAlive(ActorEndpoint endpoint)
        {
            if (keepAliveTimeout == TimeSpan.Zero)
                return;

            endpoint.DelayDeactivation(keepAliveTimeout);
        }

        public bool Equals(ActorType other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(Code, other.Code));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj.GetType() == GetType() && Equals((ActorType) obj));
        }

        public static bool operator ==(ActorType left, ActorType right) => Equals(left, right);
        public static bool operator !=(ActorType left, ActorType right) => !Equals(left, right);

        public override int GetHashCode() => Code.GetHashCode();
        public override string ToString() => Code;
    }

    static class ActorTypeActorSystemExtensions
    {
        internal static ActorRef ActorOf(this IActorSystem system, ActorType type, string id)
        {
            return system.ActorOf(ActorPath.From(type.Code, id));
        }
    }
}