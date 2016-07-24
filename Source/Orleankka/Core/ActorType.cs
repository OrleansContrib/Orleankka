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
        static readonly Dictionary<string, ActorType> types =
                    new Dictionary<string, ActorType>();

        public static void Reset() => types.Clear();

        public static void Register(IEnumerable<EndpointConfiguration> configs)
        {
            var actors = EndpointDeclaration.Generate(configs);

            foreach (var actor in actors)
                Register(actor);
       }

        static void Register(ActorType actor)
        {
            var registered = types.Find(actor.Name);
            if (registered != null)
                throw new ArgumentException(
                    $"An actor with type '{actor.Name}' has been already registered");

            types.Add(actor.Name, actor);
        }

        public readonly string Name;

        readonly bool sticky;
        readonly TimeSpan keepAliveTimeout;
        readonly Func<object, bool> reentrant;
        readonly Func<string, object> factory;
        readonly Func<ActorPath, IActorRuntime, Func<object, Task<object>>> receiver;

        internal ActorType(string name, TimeSpan keepAliveTimeout, bool sticky, Func<object, bool> reentrant, Type @interface, Func<ActorPath, IActorRuntime, Func<object, Task<object>>> receiver)
        {
            Name = name;
            this.sticky = sticky;
            this.keepAliveTimeout = sticky ? TimeSpan.FromDays(365 * 10) : keepAliveTimeout;
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

        public static ActorType Registered(string type)
        {
            var result = types.Find(type);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map type '{type}' to the corresponding actor type. " +
                     "Make sure that you've registered the assembly containing this type");

            return result;
        }

        internal IActorEndpoint Proxy(ActorPath path) => 
            (IActorEndpoint) factory(path.Serialize());

        public Func<object, Task<object>> Receiver(ActorPath path, IActorRuntime runtime) => 
            receiver(path, runtime);

        internal bool IsReentrant(object message) => 
            reentrant(message);

        internal void KeepAlive(ActorEndpoint endpoint)
        {
            if (keepAliveTimeout == TimeSpan.Zero)
                return;

            endpoint.DelayDeactivation(keepAliveTimeout);
        }

        public bool Sticky => sticky;

        public bool Equals(ActorType other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) 
                    || string.Equals(Name, other.Name));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) 
                    || obj.GetType() == GetType() && Equals((ActorType) obj));
        }

        public static bool operator ==(ActorType left, ActorType right) => Equals(left, right);
        public static bool operator !=(ActorType left, ActorType right) => !Equals(left, right);

        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }
}