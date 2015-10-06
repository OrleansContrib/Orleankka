using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Orleankka
{
    using Core;
    using Utility;

    public abstract class Ref
    {
        static readonly Dictionary<Type, Func<string, Ref>> deserializers = 
                    new Dictionary<Type, Func<string, Ref>>();

        internal static void Reset() => deserializers.Clear();

        internal static void Register(ActorType type)
        {
            var @ref = typeof(ActorRef<>).MakeGenericType(type.Implementation);

            var constructor = CompileConstructor(@ref);

            deserializers.Add(@ref, path => constructor(ActorRef.Deserialize(path)));
        }

        static Func<ActorRef, Ref> CompileConstructor(Type type)
        {
            var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
            Debug.Assert(constructor != null);

            var param = Expression.Parameter(typeof(ActorRef), "@ref");
            var lambda = Expression.Lambda<Func<ActorRef, Ref>>(Expression.New(constructor, param), param);

            return lambda.Compile();
        }

        public static Ref Deserialize(string path, Type type)
        {
            if (type == typeof(ClientRef))
                return ClientRef.Deserialize(path);

            if (type == typeof(ActorRef))
                return ActorRef.Deserialize(path);

            if (type == typeof(StreamRef))
                return StreamRef.Deserialize(path);

            var deserializer = deserializers.Find(type);
            if (deserializer != null)
                return deserializer(path);

            throw new InvalidOperationException("Unknown ref type: " + type);
        }

        public abstract string Serialize();
    }
}