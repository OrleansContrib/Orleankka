using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Orleans;
using Orleans.CodeGeneration;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    using Utility;
    using Endpoints;
    using Annotations;

    public class ActorType : IEquatable<ActorType>
    {
        static readonly Dictionary<string, ActorType> types =
                    new Dictionary<string, ActorType>();

        internal static void Reset() => types.Clear();

        internal static void Register(bool client, IEnumerable<EndpointConfiguration> configs)
        {
            var actors = EndpointDeclaration.Generate(client, configs);

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

        internal readonly string Name;
        internal readonly IActorInvoker Invoker;

        readonly TimeSpan keepAliveTimeout;
        readonly Func<object, bool> interleavePredicate;
        readonly Func<string, object> factory;
        readonly Func<ActorPath, IActorRuntime, Actor> activator;

        internal ActorType(bool client, string name, TimeSpan keepAliveTimeout, bool sticky, Func<object, bool> interleavePredicate, Type @interface, Type implementation, Func<ActorPath, IActorRuntime, Actor> activator, string invoker)
        {
            Name = name;
            this.Sticky = sticky;
            this.keepAliveTimeout = sticky ? TimeSpan.FromDays(365 * 10) : keepAliveTimeout;
            this.interleavePredicate = interleavePredicate;
            this.activator = activator;
            this.factory = Bind(@interface);
            Invoker = client ? null : InvocationPipeline.Instance.GetInvoker(implementation, invoker);
            Init(implementation);
        }

        void Init(Type implementation)
        {
            Debug.Assert(implementation.BaseType != null);
            var field = implementation.BaseType.GetField("type", BindingFlags.NonPublic | BindingFlags.Static);

            Debug.Assert(field != null);
            field.SetValue(null, this);
        }

        static Func<string, object> Bind(Type type)
        {
            var method = typeof(GrainFactory).GetMethod("GetGrain", new[] { typeof(string), typeof(string) });
            var invoker = method.MakeGenericMethod(type);
            var instance = Activator.CreateInstance(typeof(GrainFactory), nonPublic: true);
            return x => invoker.Invoke(instance, new object[] { x, null });
        }

        internal static ActorType Registered(string type)
        {
            var result = types.Find(type);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map type '{type}' to the corresponding actor type. " +
                     "Make sure that you've registered the assembly containing this type");

            return result;
        }

        internal IActorEndpoint Proxy(ActorPath path) => 
            (IActorEndpoint) factory(path.Id);

        internal Actor Activate(ActorPath path, IActorRuntime runtime) => 
            activator(path, runtime);

        [UsedImplicitly]
        internal bool MayInterleave(InvokeMethodRequest request)
        {
            var receiveMessage = request.Arguments.Length == 1;
            if (receiveMessage)
                return interleavePredicate(UnwrapImmutable(request.Arguments[0]));

            var streamMessage = request.Arguments.Length == 4;
            return streamMessage && interleavePredicate(UnwrapImmutable(request.Arguments[1]));
        }

        static object UnwrapImmutable(object item) => 
            item is Immutable<object> ? ((Immutable<object>)item).Value : item;

        internal void KeepAlive(ActorEndpoint endpoint)
        {
            if (keepAliveTimeout == TimeSpan.Zero)
                return;

            endpoint.DelayDeactivation(keepAliveTimeout);
        }

        internal bool Sticky { get; }

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