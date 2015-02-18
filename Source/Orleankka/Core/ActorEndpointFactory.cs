using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Orleankka.Core
{
    using Codegen;

    static class ActorEndpointDynamicFactory
    {
        readonly static Dictionary<Type, Func<string, object>> factories =
                    new Dictionary<Type, Func<string, object>>();

        public static IActorEndpoint Proxy(ActorPath path)
        {
            var factory = factories[path.Type];
            return (IActorEndpoint) factory(path.Id);
        }
    
        public static void Register(Type type)
        {
            var declaration = ActorEndpointDeclaration.From(type);
            factories.Add(type, Bind(declaration.ToString()));
        }

        static Func<string, object> Bind(string name)
        {
            #if PACKAGE
                const string assemblyName = "Orleankka";
            #else
                const string assemblyName = "Orleankka.Core";
            #endif

            var factory = Type.GetType(
                "Orleankka.Core.Hardcore." 
                + name + ".ActorEndpointFactory, " 
                + assemblyName);
            
            Debug.Assert(factory != null);

            var getGrainMethod = factory.GetMethod("GetGrain",
                BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(string) }, null);

            var parameter = Expression.Parameter(typeof(string), "primaryKey");
            var call = Expression.Call(getGrainMethod, new Expression[] { parameter });
            
            return Expression.Lambda<Func<string, object>>(call, parameter).Compile();
        }

        public static void Reset()
        {
            factories.Clear();
        }
    }
}