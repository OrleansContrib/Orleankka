﻿using System;
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

        internal static IActorActivator Activator = new DefaultActorActivator();

        internal static void Register(IEnumerable<Assembly> assemblies)
        {
            var actors = ActorTypeDeclaration.Generate(assemblies.ToArray());

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

        public static IEnumerable<ActorType> Registered() => types.Values;

        internal readonly string Name;
        internal readonly IActorInvoker Invoker;

        readonly Type actor;
        readonly TimeSpan keepAliveTimeout;
        readonly Func<object, bool> interleavePredicate;
        readonly Dispatcher dispatcher;

        internal ActorType(Type actor, Type endpoint)
        {
            this.actor = actor;

            Name = ActorTypeName.Of(actor);
            Sticky = StickyAttribute.IsApplied(actor);
            keepAliveTimeout = Sticky ? TimeSpan.FromDays(365 * 10) : KeepAliveAttribute.Timeout(actor);
            Invoker = InvocationPipeline.Instance.GetInvoker(actor, InvokerAttribute.From(actor));

            interleavePredicate = ReentrantAttribute.MayInterleavePredicate(actor);
            
            dispatcher = new Dispatcher(actor);
            dispatchers.Add(actor, dispatcher);

            Init(endpoint);
        }

        void Init(Type grain)
        {
            Debug.Assert(grain.BaseType != null);
            var field = grain.BaseType.GetField("type", BindingFlags.NonPublic | BindingFlags.Static);

            Debug.Assert(field != null);
            field.SetValue(null, this);
        }

        internal Actor Activate(IActorHost host, ActorPath path, IActorRuntime runtime)
        {
            var instance = Activator.Activate(actor, path.Id, runtime, dispatcher);
            instance.Initialize(host, path, runtime, dispatcher);
            return instance;
        }

        [UsedImplicitly]
        internal bool MayInterleave(InvokeMethodRequest request)
        {
            if (request?.Arguments == null)
                return false;

            var receiveMessage = request.Arguments.Length == 1;
            if (receiveMessage)
                return interleavePredicate(UnwrapImmutable(request.Arguments[0]));

            var streamMessage = request.Arguments.Length == 4;
            return streamMessage && interleavePredicate(UnwrapImmutable(request.Arguments[1]));
        }

        static object UnwrapImmutable(object item) => 
            item is Immutable<object> ? ((Immutable<object>)item).Value : item;

        internal void KeepAlive(IActorHost host)
        {
            if (keepAliveTimeout == TimeSpan.Zero)
                return;

            host.DelayDeactivation(keepAliveTimeout);
        }

        internal IEnumerable<StreamSubscriptionSpecification> Subscriptions() => 
            StreamSubscriptionSpecification.From(actor, dispatcher);

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