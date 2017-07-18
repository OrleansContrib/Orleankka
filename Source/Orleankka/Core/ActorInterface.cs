using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Orleans;

namespace Orleankka.Core
{
    using Utility;

    class ActorInterface
    {
        static readonly Dictionary<string, ActorInterface> interfaces =
                    new Dictionary<string, ActorInterface>();

        internal static void Register(IEnumerable<Assembly> assemblies, IEnumerable<ActorInterfaceMapping> mappings)
        {
            var unregistered = new List<ActorInterfaceMapping>();

            foreach (var each in mappings)
            {
                var existing = interfaces.Find(each.TypeName);
                if (existing == null)
                {
                    unregistered.Add(each);
                    continue;
                }

                if (existing.Mapping != each)
                    throw new DuplicateActorTypeException(existing.Mapping, each);
            }

            using (Trace.Execution("Generation of actor interface assemblies"))
            {
                var generated = ActorInterfaceDeclaration.Generate(assemblies, unregistered);

                foreach (var each in generated)
                    interfaces[each.Mapping.TypeName] = each;
            }
        }

        internal readonly ActorInterfaceMapping Mapping;
        internal readonly Type Grain;

        Func<IGrainFactory, string, object> factory;

        internal ActorInterface(ActorInterfaceMapping mapping, Type grain)
        {
            Mapping = mapping;
            Grain = grain;

            Array.ForEach(mapping.Types, ActorTypeName.Register);
        }

        public Assembly GrainAssembly() => Grain.Assembly;

        internal static void Bind(IGrainFactory factory)
        {            
            foreach (var @interface in interfaces.Values)
            {
                var method = factory.GetType().GetMethod("GetGrain", new[] {typeof(string), typeof(string)});
                var invoker = method.MakeGenericMethod(@interface.Grain);

                var @this = Expression.Parameter(typeof(object));
                var id = Expression.Parameter(typeof(string));
                var ns = Expression.Constant(null, typeof(string));

                var call = Expression.Call(Expression.Convert(@this, factory.GetType()), invoker, id, ns);
                var func = Expression.Lambda<Func<object, string, object>>(call, @this, id).Compile();

                @interface.factory = func;
            }
        }

        internal static ActorInterface Registered(string name)
        {
            var result = interfaces.Find(name);
            if (result == null)
                throw new InvalidOperationException(
                    $"Unable to map type '{name}' to the corresponding actor. " +
                     "Make sure that you've registered an actor type or the assembly containing this type");

            return result;
        }

        internal IActorEndpoint Proxy(string id, IGrainFactory instance) => (IActorEndpoint) factory(instance, id);
    }
}