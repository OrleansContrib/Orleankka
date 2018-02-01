using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        internal static void Register(ActorInvocationPipeline pipeline, Assembly[] assemblies, string[] conventions)
        {
            var unregistered = assemblies
                .SelectMany(x => x.ActorTypes())
                .Where(x => !types.ContainsKey(ActorTypeName.Of(x)));

            foreach (var each in ActorTypes(unregistered))
            {
                types.Add(each.Name, each);
                grains.Add(each.grain, each);
            }

            IEnumerable<ActorType> ActorTypes(IEnumerable<Type> types)
            {
                foreach (var each in types)
                {
                    var typeName = ActorTypeName.Of(each);
                    
                    var @interface = ActorInterface.Of(typeName);
                    if (@interface == null)
                        continue;

                    var middleware = pipeline.Middleware(each);
                    yield return new ActorType(each, @interface, each, middleware, conventions);
                }
            }
        }

        public static IEnumerable<ActorType> Registered() => types.Values;
        internal string Name => Interface.Mapping.TypeName;

        public readonly Type Class;
        public readonly ActorInterface Interface;
        internal readonly Dispatcher dispatcher;
        
        readonly Type grain;
        public readonly IActorMiddleware Middleware;

        ActorType(Type @class, ActorInterface @interface, Type grain, IActorMiddleware middleware, string[] conventions)
        {
            this.grain = grain;
            
            Class = @class;
            Interface = @interface;
            Middleware = middleware;            
            
            dispatcher = new Dispatcher(@class, conventions);
            dispatchers.Add(@class, dispatcher);
        }

        internal IEnumerable<StreamSubscriptionSpecification> Subscriptions() => StreamSubscriptionSpecification.From(Class);
    }
}