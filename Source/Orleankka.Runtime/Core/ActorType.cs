using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleans;
using Orleans.Internals;

namespace Orleankka.Core
{
    using Utility;

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

        internal static ActorType Of(Type grainType)
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
                grains.Add(each.grain, each);
                typeCodes.Add(each.typeCode, each);
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
        internal readonly Dispatcher dispatcher;
        
        readonly int typeCode;
        readonly Type grain;
        readonly TimeSpan keepAliveTimeout;
        readonly string invoker;

        ActorType(Type @class, ActorInterface @interface, Type grain, string[] conventions)
        {
            this.grain = grain;
            
            Class = @class;
            Interface = @interface;
            typeCode = grain.TypeCode();
            
            keepAliveTimeout = KeepAliveAttribute.Timeout(@class);
            invoker = InvokerAttribute.From(@class);
            
            dispatcher = new Dispatcher(@class, conventions);
            dispatchers.Add(@class, dispatcher);
        }

        internal IActorInvoker Invoker(ActorInvocationPipeline pipeline) => 
            pipeline.GetInvoker(Class, invoker);
        
        internal void KeepAlive(Grain grain)
        {
            if (keepAliveTimeout == TimeSpan.Zero)
                return;

            grain.Runtime().DelayDeactivation(grain, keepAliveTimeout);
        }

        internal IEnumerable<StreamSubscriptionSpecification> Subscriptions() => 
            StreamSubscriptionSpecification.From(Class, dispatcher);
    }
}