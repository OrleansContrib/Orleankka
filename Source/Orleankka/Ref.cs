using System;
using System.Linq.Expressions;
using System.Collections.Generic;
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

        internal static void Register(ActorType actor)
        {
            var @ref = ConstructRefType(actor);
            var constructor = CompileConstructor(@ref);
            RegisterDeserializer(@ref, constructor);
        }

        static Type ConstructRefType(ActorType actor)
        {
            return typeof(ActorRef<>).MakeGenericType(actor.Implementation);
        }

        static Func<ActorRef, Ref> CompileConstructor(Type type)
        {
            var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];

            var parameter = Expression.Parameter(typeof(ActorRef), "@ref");
            var @new      = Expression.New(constructor, parameter);
            var invoker   = Expression.Lambda<Func<ActorRef, Ref>>(@new, parameter);

            return invoker.Compile();
        }

        static void RegisterDeserializer(Type @ref, Func<ActorRef, Ref> constructor)
        {
            deserializers.Add(@ref, path => constructor(ActorRef.Deserialize(path)));
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