using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Orleans;
using Orleans.CodeGeneration;
using Orleans.Internals;
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
        
        static readonly Dictionary<Type, ActorType> grains =
                    new Dictionary<Type, ActorType>();

        static readonly Dictionary<int, ActorType> typeCodes =
                    new Dictionary<int, ActorType>();

        public static ActorType Of<T>() => Of(typeof(T));
        public static ActorType Of(Type type) => Of(ActorTypeName.Of(type));
        
        internal static ActorType OfGrain(Type grainType)
        {
            Requires.NotNull(grainType, nameof(grainType));

            var result = grains.Find(grainType);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map grain type '{grainType}' to the corresponding actor implementation class");

            return result;
        }

        public static ActorType Of(string name)
        {
            Requires.NotNull(name, nameof(name));

            var result = types.Find(name);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map actor type name '{name}' to the corresponding actor implementation class");

            return result;
        }

        public static ActorType Of(int typeCode)
        {
            var result = typeCodes.Find(typeCode);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map actor type code '{typeCode}' to the corresponding actor type");

            return result;
        }
        
        internal static void Register(Assembly[] assemblies, string[] conventions)
        {
            var unregistered = assemblies
                .SelectMany(x => x.ActorTypes())
                .Where(x => !types.ContainsKey(ActorTypeName.Of(x)));

            foreach (var each in ActorTypes(unregistered))
            {
                types.Add(each.Name, each);
                grains.Add(each.Grain, each);
                typeCodes.Add(each.TypeCode, each);
                typeCodes.Add(each.Interface.TypeCode, each);
            }

            IEnumerable<ActorType> ActorTypes(IEnumerable<Type> types)
            {
                foreach (var each in types)
                {
                    var typeName = ActorTypeName.Of(each);
                    var @interface = ActorInterface.Of(typeName);
                    if (@interface == null)
                        continue;
                    yield return new ActorType(each, @interface, each, conventions);
                }
            }
        }

        public static IEnumerable<ActorType> Registered() => types.Values;
        internal string Name => Interface.Mapping.TypeName;

        public readonly Type Class;
        public readonly ActorInterface Interface;
        public readonly int TypeCode;
        internal readonly Type Grain;

        readonly TimeSpan keepAliveTimeout;
        readonly Func<object, bool> interleavePredicate;
        readonly string invoker;
        internal readonly Dispatcher dispatcher;

        internal ActorType(Type @class, ActorInterface @interface, Type grain, string[] conventions)
        {
            Class = @class;
            Interface = @interface;
            Grain = grain;
            TypeCode = grain.TypeCode();
            
            Sticky = StickyAttribute.IsApplied(@class);
            keepAliveTimeout = Sticky ? TimeSpan.FromDays(365 * 10) : KeepAliveAttribute.Timeout(@class);
            interleavePredicate = InterleaveAttribute.MayInterleavePredicate(@class);
            invoker = InvokerAttribute.From(@class);
            
            dispatcher = new Dispatcher(@class, conventions);
            dispatchers.Add(@class, dispatcher);
        }

        internal ActorGrain Activate(IActorHost host, ActorPath path, IActorRuntime runtime, IActorActivator activator)
        {
            var instance = activator.Activate(Class, path.Id, runtime, dispatcher);
            instance.Initialize(host, path, runtime, dispatcher);
            return instance;
        }

        internal IActorInvoker Invoker(ActorInvocationPipeline pipeline) => 
            pipeline.GetInvoker(Class, invoker);
        
        /// <summary> 
        /// FOR INTERNAL USE ONLY!
        /// </summary>
        [UsedImplicitly]
        public static bool MayInterleave(string typeName, InvokeMethodRequest request) => 
            Of(typeName).MayInterleave(request);

        bool MayInterleave(InvokeMethodRequest request)
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

        internal void KeepAlive(Grain grain)
        {
            if (keepAliveTimeout == TimeSpan.Zero)
                return;

            grain.Runtime().DelayDeactivation(grain, keepAliveTimeout);
        }

        internal IEnumerable<StreamSubscriptionSpecification> Subscriptions() => 
            StreamSubscriptionSpecification.From(Class, dispatcher);

        internal bool Sticky { get; }
    }
}