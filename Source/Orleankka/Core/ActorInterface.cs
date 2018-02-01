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

        public static ActorInterface Of(Type type) => Of(ActorTypeName.Of(type));

        public static ActorInterface Of(string name)
        {
            Requires.NotNull(name, nameof(name));
            return names.Find(name);
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

            Bind();
        }

        public readonly string Name;

        internal readonly ActorInterfaceMapping Mapping;
        internal readonly Type Grain;

        Func<IGrainFactory, string, object> factory;

        ActorInterface(ActorInterfaceMapping mapping, Type grain)
        {
            Name = Mapping.TypeName;

            Mapping = mapping;
            Grain = grain;

            Array.ForEach(mapping.Types, ActorTypeName.Register);
        }

        static void Bind()
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