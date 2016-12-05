using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Orleans.CodeGeneration;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    using Utility;
    using Annotations;

    public class ActorType : IEquatable<ActorType>
    {
        internal static string[] Conventions;

        internal static Dispatcher Dispatcher(Type actor) =>
            dispatchers.Find(actor) ?? new Dispatcher(actor, Conventions);

        static readonly Dictionary<Type, Dispatcher> dispatchers =
                    new Dictionary<Type, Dispatcher>();

        static readonly Dictionary<string, ActorType> types =
                    new Dictionary<string, ActorType>();

        internal static IActorActivator Activator;

        static ActorType()
        {
            Reset();
        }

        internal static void Reset()
        {
            Activator = new DefaultActorActivator();
            Conventions = null;
            types.Clear();
        }

        internal static void Register(IEnumerable<Assembly> assemblies, IEnumerable<ActorConfiguration> configs)
        {
            var actors = ClassDeclaration.Generate(assemblies, configs);

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

        readonly Type actor;
        readonly TimeSpan keepAliveTimeout;
        readonly Func<object, bool> interleavePredicate;
        readonly Dispatcher dispatcher;

        internal ActorType(string name, TimeSpan keepAliveTimeout, bool sticky, Func<object, bool> interleavePredicate, Type grain, Type actor, string invoker)
        {
            Name = name;

            this.Sticky = sticky;
            this.keepAliveTimeout = sticky ? TimeSpan.FromDays(365 * 10) : keepAliveTimeout;
            this.interleavePredicate = interleavePredicate;
            this.actor = actor;

            Invoker = InvocationPipeline.Instance.GetInvoker(actor, invoker);

            dispatcher = new Dispatcher(actor);
            dispatchers.Add(actor, dispatcher);

            Init(grain);
        }

        void Init(Type grain)
        {
            Debug.Assert(grain.BaseType != null);
            var field = grain.BaseType.GetField("type", BindingFlags.NonPublic | BindingFlags.Static);

            Debug.Assert(field != null);
            field.SetValue(null, this);
        }

        internal Actor Activate(ActorEndpoint endpoint, ActorPath path, IActorRuntime runtime)
        {
            var instance = Activator.Activate(actor, path.Id, runtime, dispatcher);
            instance.Initialize(endpoint, path, runtime, dispatcher);
            return instance;
        }

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