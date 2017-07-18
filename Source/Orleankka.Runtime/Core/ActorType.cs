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

    public class ActorType
    {
        internal static Dispatcher Dispatcher(Type actor) => dispatchers.Find(actor) ?? new Dispatcher(actor);

        static readonly Dictionary<Type, Dispatcher> dispatchers =
                    new Dictionary<Type, Dispatcher>();

        static readonly Dictionary<string, ActorType> types =
                    new Dictionary<string, ActorType>();

        internal static IActorActivator Activator = new DefaultActorActivator();

        internal static void Register(Assembly[] assemblies, string[] conventions)
        {
            var unregistered = assemblies
                .SelectMany(x => x.ActorTypes())
                .Where(x => !types.ContainsKey(ActorTypeName.Of(x)));

            var actors = ActorTypeDeclaration.Generate(assemblies.ToArray(), unregistered, conventions);

            foreach (var actor in actors)
                types.Add(actor.Name, actor);
        }

        internal static IEnumerable<ActorType> Registered() => types.Values;

        internal string Name => @interface.Mapping.TypeName;
        internal readonly IActorInvoker Invoker;

        readonly Type actor;
        readonly ActorInterface @interface;
        readonly TimeSpan keepAliveTimeout;
        readonly Func<object, bool> interleavePredicate;
        readonly Dispatcher dispatcher;

        internal ActorType(Type actor, ActorInterface @interface, Type endpoint, string[] conventions)
        {
            this.actor = actor;
            this.@interface = @interface;

            Sticky = StickyAttribute.IsApplied(actor);
            keepAliveTimeout = Sticky ? TimeSpan.FromDays(365 * 10) : KeepAliveAttribute.Timeout(actor);
            Invoker = InvocationPipeline.Instance.GetInvoker(actor, InvokerAttribute.From(actor));

            interleavePredicate = ReentrantAttribute.MayInterleavePredicate(actor);
            
            dispatcher = new Dispatcher(actor, conventions);
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
        public bool MayInterleave(InvokeMethodRequest request)
        {
            if (request?.Arguments == null)
                return false;

            var receiveMessage = request.Arguments.Length == 1;
            if (receiveMessage)
                return interleavePredicate(UnwrapImmutable(request.Arguments[0]));

            var streamMessage = request.Arguments.Length == 5;
            return streamMessage && interleavePredicate(UnwrapImmutable(request.Arguments[2]));
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
    }
}