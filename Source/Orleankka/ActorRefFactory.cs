using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Orleankka
{
    static class ActorRefFactory
    {
        static readonly ConcurrentDictionary<Type, Func<string, IActor>> cache =
                    new ConcurrentDictionary<Type, Func<string, IActor>>();

        public static IActor Create(ActorPath path)
        {
            var factory = FactoryOf(path.Type);
            return factory(path.Id);
        }

        static Func<string, IActor> FactoryOf(Type type)
        {
            return cache.GetOrAdd(type, t =>
            {
                var factory = t.Assembly
                    .ExportedTypes
                    .Where(IsOrleansCodegenedFactory)
                    .SingleOrDefault(x => x.GetMethod("Cast").ReturnType == t);

                if (factory == null)
                    throw new ApplicationException("Can't find factory class for " + t);

                return Bind(factory);
            });
        }

        static bool IsOrleansCodegenedFactory(Type type)
        {
            return type.GetCustomAttributes(typeof(GeneratedCodeAttribute), true)
                       .Cast<GeneratedCodeAttribute>()
                       .Any(x => x.Tool == "Orleans-CodeGenerator")
                   && type.Name.EndsWith("Factory");
        }

        static Func<string, IActor> Bind(IReflect factory)
        {
            var method = factory.GetMethod("GetGrain",
                BindingFlags.Public | BindingFlags.Static, null,
                new[] {typeof(string)}, null);

            var argument = Expression.Parameter(typeof(string), "primaryKey");
            var call = Expression.Call(method, new Expression[] {argument});
            var lambda = Expression.Lambda<Func<string, IActor>>(call, argument);

            return lambda.Compile();
        }
    }
}