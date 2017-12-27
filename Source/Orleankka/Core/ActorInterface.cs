using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Orleans;
using Orleans.Internals;

namespace Orleankka.Core
{
    using Utility;

    public class ActorInterface
    {
        static readonly Dictionary<string, ActorInterface> names =
                    new Dictionary<string, ActorInterface>();

        public static ActorInterface Of<T>() => Of(typeof(T));
        public static ActorInterface Of(Type type) => Of(ActorTypeName.Of(type));

        public static ActorInterface Of(string name)
        {
            Requires.NotNull(name, nameof(name));

            var result = names.Find(name);

            /*
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map actor type name '{name}' to the corresponding actor. " +
                        "Make sure that you've registered an actor type or the assembly containing this type");
            */
            return result;
        }

        internal static void Register(IEnumerable<ActorInterfaceMapping> mappings)
        {
            var unregistered = new List<ActorInterfaceMapping>();

            foreach (var each in mappings)
            {
                var existing = names.Find(each.TypeName);
                if (existing == null)
                {
                    unregistered.Add(each);
                    continue;
                }

                if (existing.Mapping != each)
                    throw new DuplicateActorTypeException(existing.Mapping, each);
            }

            foreach (var each in unregistered)
            {
                var @interface = new ActorInterface(each, each.CustomInterface);
                names.Add(@interface.Name, @interface);
            }
        }

        internal readonly ActorInterfaceMapping Mapping;
        internal readonly Type Grain;

        public readonly int TypeCode;
        public readonly string Name;

        Func<IGrainFactory, string, object> factory;

        internal ActorInterface(ActorInterfaceMapping mapping, Type grain)
        {
            Mapping = mapping;
            Grain = grain;

            TypeCode = grain.TypeCode();
            grain.InterfaceVersion();
            Name = Mapping.TypeName;
            
            Array.ForEach(mapping.Types, ActorTypeName.Register);
        }

        internal static void Bind()
        {            
            foreach (var @interface in names.Values)
            {
                var method = typeof(IGrainFactory).GetMethod("GetGrain", new[] {typeof(string), typeof(string)});
                var invoker = method.MakeGenericMethod(@interface.Grain);

                var @this = Expression.Parameter(typeof(object));
                var id = Expression.Parameter(typeof(string));
                var ns = Expression.Constant(null, typeof(string));

                var call = Expression.Call(Expression.Convert(@this, typeof(IGrainFactory)), invoker, id, ns);
                var func = Expression.Lambda<Func<object, string, object>>(call, @this, id).Compile();

                @interface.factory = func;
            }
        }

        internal IActorGrain Proxy(string id, IGrainFactory instance) => (IActorGrain) factory(instance, id);
    }
}