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
        static readonly ConcurrentDictionary<Type, Func<string, object>> cache =
                    new ConcurrentDictionary<Type, Func<string, object>>();

        public static ActorRef Create(ActorPath path)
        {
            return new ActorRef((IActor)cache.GetOrAdd(path.Type, type =>
            {
                var factory = type.Assembly
                                   .ExportedTypes
                                   .Where(IsOrleansCodegenedFactory)
                                   .SingleOrDefault(x => x.GetMethod("Cast").ReturnType == type);

                if (factory == null)
                    throw new ApplicationException("Can't find factory class for " + type);

                return Bind(factory);
            })
            (path.Id));
        }

        static bool IsOrleansCodegenedFactory(Type type)
        {
            return type.GetCustomAttributes(typeof(GeneratedCodeAttribute), true)
                       .Cast<GeneratedCodeAttribute>()
                       .Any(x => x.Tool == "Orleans-CodeGenerator")
                   && type.Name.EndsWith("Factory");
        }

        static Func<string, object> Bind(IReflect factory)
        {
            var method = factory.GetMethod("GetGrain",
                BindingFlags.Public | BindingFlags.Static, null,
                new[] {typeof(string)}, null);

            var argument = Expression.Parameter(typeof(string), "primaryKey");
            var call = Expression.Call(method, new Expression[] {argument});
            var lambda = Expression.Lambda<Func<string, object>>(call, argument);

            return lambda.Compile();
        }
    }
}